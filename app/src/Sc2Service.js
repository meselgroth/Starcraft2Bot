export class Sc2Service {
  async getBitmap() {
    try {
      let response = await fetch('/api/bitmap');
      if (response.ok) {
        return await response.blob();
      }
      return `Error response[${response.status}]: ${response.statusText}`;
    } catch (error) {
      return `${error.name}: ${error.message}`;
    }
  }
  async getJson(endpoint) {
    try {
      let response = await fetch('api/' + endpoint);
      if (response.ok) {
        return await response.json();
      }
      return `Error response[${response.status}]: ${response.statusText}`;
    } catch (error) {
      return `${error.name}: ${error.message}`;
    }
  }
}
