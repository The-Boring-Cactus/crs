export class Emitter {
  constructor() {
    this.listeners = new Map()
  }
  /**
   * Add event listener
   * @param label  Event name
   * @param callback  Callback
   * @param vm  this object
   * @return {boolean}
   */
  addListener(label, callback, vm) {
    if (typeof callback === "function") {
      // add if label does not exist
      this.listeners.has(label) || this.listeners.set(label, [])
      // Add callback function to label
      this.listeners.get(label).push({ callback: callback, vm: vm })
      return true
    }
    return false
  }

  /**
   * Remove monitor
   * @param label  Event name
   * @param callback Callback
   * @param vm this object
   * @return {boolean}
   */
  removeListener(label, callback, vm) {
    // Get the current event from the listener list
    const listeners = this.listeners.get(label)
    let index

    if (listeners && listeners.length) {
      // Find the position of the current event in the event monitoring list
      index = listeners.reduce((i, listener, index) => {
        if (
          typeof listener.callback === "function" &&
          listener.callback === callback &&
          listener.vm === vm
        ) {
          i = index
        }
        return i
      }, -1)

      if (index > -1) {
        //  Remove event
        listeners.splice(index, 1)
        this.listeners.set(label, listeners)
        return true
      }
    }
    return false
  }

  /**
   * Trigger monitor
   * @param label Event name
   * @param args parameter
   * @return {boolean}
   */
  emit(label, ...args) {
    // Get events stored in the event list
    const listeners = this.listeners.get(label)

    if (listeners && listeners.length) {
      listeners.forEach(listener => {
        // Extend the callback function to have methods in listener.vm
        listener.callback.call(listener.vm, ...args)
      })
      return true
    }
    return false
  }
}

export default new Emitter()
