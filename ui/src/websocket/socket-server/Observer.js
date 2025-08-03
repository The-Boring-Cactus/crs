import Emitter from "./Emitter"

export default class {
  reconnectTimeoutId = 0 // Reconnect timeout id
  reconnectionCount = 0 // Reconnected times
  /**
   * Observer mode, websocket service core function package
   * @param connectionUrl url
   * @param opts Other configuration items
   */
  constructor(connectionUrl, opts = { format: "" }) {
    // Get the format in the parameter and convert it to lowercase
    this.format = opts.format && opts.format.toLowerCase()

    //If the URL starts with // to process it, add the correct websocket protocol prefix
    if (connectionUrl.startsWith("//")) {
      // If the current website is an https request, add the wss prefix, otherwise add the ws prefix
      const scheme = window.location.protocol === "https:" ? "wss" : "ws"
      connectionUrl = `${scheme}:${connectionUrl}`
    }
    // Assign the processed url and opts to the internal variables of the current class
    this.connectionUrl = connectionUrl
    this.opts = opts
    this.reconnection = this.opts.reconnection || false
    this.reconnectionAttempts = this.opts.reconnectionAttempts || Infinity
    this.reconnectionDelay = this.opts.reconnectionDelay || 1000
    this.passToStoreHandler = this.opts.passToStoreHandler

    // establish connection
    this.connect(connectionUrl, opts)

    // If store is passed in the configuration parameters, store will be assigned
    if (opts.store) {
      this.store = opts.store
    }
    // If there is a synchronization processing function that passes vuex in the configuration parameters, assign mutations
    if (opts.mutations) {
      this.mutations = opts.mutations
    }
 
    this.onEvent()
  }

  // Connect websocket
  connect(connectionUrl, opts = { format: "" }) {
    // Get the protocol passed in the configuration parameter
    const protocol = opts.protocol || ""
    // If no protocol is passed, establish a normal websocket connection, otherwise, create a websocket connection with protocol
    this.WebSocket =
      opts.WebSocket ||
      (protocol === ""
        ? new WebSocket(connectionUrl)
        : new WebSocket(connectionUrl, protocol))
    //Enable json sending
    if (this.format === "json") {
      // If there is no sen Obj in websocket, add this method object
      if (!("sendObj" in this.WebSocket)) {
        // Convert the sent message into a json string
        this.WebSocket.sendObj = obj => this.WebSocket.send(JSON.stringify(obj))
      }
    }
    return this.WebSocket
  }
  // reconnect
  reconnect() {
    // Reconnect when the number of reconnections is less than or equal to the set connection times
    if (this.reconnectionCount <= this.reconnectionAttempts) {
      this.reconnectionCount++
      // Clear the timer of the last reconnection
      window.clearTimeout(this.reconnectTimeoutId)
    
      this.reconnectTimeoutId = window.setTimeout(() => {
        // If vuex is enabled, the reconnection method in vuex is triggered
        if (this.store) {
          this.passToStore("SOCKET_RECONNECT", this.reconnectionCount)
        }
        // reconnect
        this.connect(this.connectionUrl, this.opts)
        // Trigger Web Socket events
        this.onEvent()
      }, this.reconnectionDelay)
    } else {
      // If vuex is enabled, the reconnection failure method is triggered
      if (this.store) {
        this.passToStore("SOCKET_RECONNECT_ERROR", true)
      }
    }
  }

  // Event distribution
  onEvent() {
    ;["onmessage", "onclose", "onerror", "onopen"].forEach(eventType => {
      // eslint-disable-next-line @typescript-eslint/ban-ts-ignore
      // @ts-ignore
      this.WebSocket[eventType] = event => {
        Emitter.emit(eventType, event)

        // Call the corresponding method in vuex
        if (this.store) {
          this.passToStore("SOCKET_" + eventType, event)
        }

        // Execute when the event is onopen in the reconnect state
        if (this.reconnection && eventType === "onopen") {
          // Setting example
          this.opts.$setInstance && this.opts.$setInstance(event.currentTarget)
          // Empty reconnection times
          this.reconnectionCount = 0
        }

        // If in the reconnect state and the event is onclose, call the reconnect method
        if (this.reconnection && eventType === "onclose") {
          this.reconnect()
        }
      }
    })
  }

  /**
   * Trigger methods in vuex
   * @param eventName 
   * @param event 
   */
  passToStore(eventName, event) {
    // If there is an event processing function in the parameter, the custom event processing function is executed, otherwise the default processing function is executed
    if (this.passToStoreHandler) {
      this.passToStoreHandler(
        eventName,
        event,
        this.defaultPassToStore.bind(this)
      )
    } else {
      this.defaultPassToStore(eventName, event)
    }
  }

  /**
   * The default event handler
   * @param eventName 
   * @param event 
   */
  defaultPassToStore(eventName, event) {
    // If the beginning of the event name is not SOCKET_ then terminate the function
    if (!eventName.startsWith("SOCKET_")) {
      return
    }
    let method = "commit"
    // Turn the letter of the event name to uppercase
    let target = eventName.toUpperCase()
    // Message content
    let msg = event
    // data exists and the data is in json format
    if (this.format === "json" && event.data) {
      // Convert data from json string to json object
      msg = JSON.parse(event.data)
      // Determine whether msg is synchronous or asynchronous
      if (msg.mutation) {
        target = [msg.namespace || "", msg.mutation].filter(e => !!e).join("/")
      } else if (msg.action) {
        method = "dispatch"
        target = [msg.namespace || "", msg.action].filter(e => !!e).join("/")
      }
    }
    if (this.mutations) {
      target = this.mutations[target] || target
    }
    // Trigger the method in the store
    if (this.store._p) {
      // pinia
      target = eventName.toUpperCase()
      this.store[target](msg)
    } else {
      // vuex
      this.store[method](target, msg)
    }
  }
}
