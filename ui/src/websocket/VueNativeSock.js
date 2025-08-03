import Observer from "./socket-server/Observer"
import Emitter from "./socket-server/Emitter"

export default {
  install(app, connection, opts = { format: "" }) {
    // No incoming connection, throw an exception
    if (!connection) {
      throw new Error("[vue-native-socket] cannot locate connection")
    }

    let observer

    opts.$setInstance = wsInstance => {
      //  Add $socket to global properties
      app.config.globalProperties.$socket = wsInstance
    }

    // Enable manual connection in configuration options
    if (opts.connectManually) {
      app.config.globalProperties.$connect = (
        connectionUrl = connection,
        connectionOpts = opts
      ) => {
        //Add a set instance to the parameters passed by the caller
        connectionOpts.$setInstance = opts.$setInstance
        //  Create Observer to establish websocket connection
        observer = new Observer(connectionUrl, connectionOpts)
        //  Add $socket globally
        app.config.globalProperties.$socket = observer.WebSocket
      }

      //  Globally add disconnection processing functions
      app.config.globalProperties.$disconnect = () => {
        if (observer && observer.reconnection) {
          // Change the reconnection status to false
          observer.reconnection = false
          // Remove the reconnection timer
          clearTimeout(observer.reconnectTimeoutId)
        }
        //  If the global attribute socket exists, remove it from the global attribute
        if (app.config.globalProperties.$socket) {
          // Close the connection
          app.config.globalProperties.$socket.close()
          delete app.config.globalProperties.$socket
        }
      }
    } else {
      // Manual connection is not enabled
      observer = new Observer(connection, opts)
      //  Add the $socket attribute globally to connect to the websocket server
      app.config.globalProperties.$socket = observer.WebSocket
    }
    const hasProxy =
      typeof Proxy !== "undefined" &&
      typeof Proxy === "function" &&
      /native code/.test(Proxy.toString())

    app.mixin({
      created() {
        const vm = this
        const sockets = this.$options["sockets"]

        if (hasProxy) {
          this.$options.sockets = new Proxy(
            {},
            {
              set(target, key, value) {
                //  Add monitor
                Emitter.addListener(key, value, vm)
                target[key] = value
                return true
              },
              deleteProperty(target, key) {
                // Remove monitor
                Emitter.removeListener(key, vm.$options.sockets[key], vm)
                delete target.key
                return true
              }
            }
          )
          app.config.globalProperties.sockets = new Proxy(
            {},
            {
              set(target, key, value) {
                // Add monitor
                Emitter.addListener(key, value, vm)
                target[key] = value
                return true
              },
              deleteProperty(target, key) {
                // Remove monitor
                Emitter.removeListener(key, vm.$options.sockets[key], vm)
                delete target.key
                return true
              }
            }
          )
          if (sockets) {
            Object.keys(sockets).forEach(key => {
              // Add the key in sockets to $options
              this.$options.sockets[key] = sockets[key]
              app.config.globalProperties.sockets[key] = sockets[key]
            })
          }
        } else {
          // Seal the object so that it cannot be changed
          Object.seal(this.$options.sockets)
          Object.seal(app.config.globalProperties.sockets)
          if (sockets) {
            Object.keys(sockets).forEach(key => {
              // Add monitor
              Emitter.addListener(key, sockets[key], vm)
            })
          }
        }
      },
      beforeUnmount() {
        if (hasProxy) {
          const sockets = this.$options["sockets"]

          if (sockets) {
            Object.keys(sockets).forEach(key => {
              // If the proxy has sockets before destruction, remove the keys added to sockets in $options
              delete this.$options.sockets[key]
              delete app.config.globalProperties.sockets
            })
          }
        }
      }
    })
  }
}

// export