export class Sc2Service {
  async getBitmap() {
    try {
      let response = await fetch('/api/bitmap');
      if (response.ok) {
        return await response.text();
      }
      return `Error response[${response.status}]: ${response.statusText}`;
    } catch (error) {
      return `${error.name}: ${error.message}`;
    }
  }
  async getObservation() {
    try {
      let response = await fetch('/response.json');
      if (response.ok) {
        return await response.json();
      }
      return `Error response[${response.status}]: ${response.statusText}`;
    } catch (error) {
      return `${error.name}: ${error.message}`;
    }
  }
}
