export class Sc2Service {
  async getObservation() {
    return await fetch('/response.json')
      .then(d => d.json());
  }
}
