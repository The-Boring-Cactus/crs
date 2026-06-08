import { defineStore } from "pinia";
import { ref } from "vue";

export const useSetupStore = defineStore("setup", () => {
  const isConfigured = ref(null); // null = loading, true/false = known
  const isLoading = ref(false);
  const error = ref(null);

  const API_BASE = "http://localhost:9876/api/setup";

  async function checkStatus() {
    isLoading.value = true;
    error.value = null;
    try {
      const response = await fetch(`${API_BASE}/status/`);
      const data = await response.json();
      isConfigured.value = data.configured || data.Configured || false;
      return isConfigured.value;
    } catch (err) {
      console.error("Failed to check setup status:", err);
      error.value = err.message;
      // If server is not reachable, assume not configured
      isConfigured.value = false;
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  async function testConnection(dbConfig) {
    isLoading.value = true;
    error.value = null;
    try {
      const response = await fetch(`${API_BASE}/test-connection/`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(dbConfig),
      });
      const data = await response.json();
      return data;
    } catch (err) {
      console.error("Connection test failed:", err);
      error.value = err.message;
      return { success: false, message: err.message };
    } finally {
      isLoading.value = false;
    }
  }

  async function completeSetup(setupData) {
    isLoading.value = true;
    error.value = null;
    try {
      const response = await fetch(`${API_BASE}/complete/`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(setupData),
      });
      const data = await response.json();
      if (data.success || data.Success) {
        isConfigured.value = true;
      }
      return data;
    } catch (err) {
      console.error("Setup completion failed:", err);
      error.value = err.message;
      return { success: false, message: err.message };
    } finally {
      isLoading.value = false;
    }
  }

  return {
    isConfigured,
    isLoading,
    error,
    checkStatus,
    testConnection,
    completeSetup,
  };
});
