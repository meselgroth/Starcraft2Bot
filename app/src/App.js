import React, { Component } from 'react';
import './App.css';
import { RawData } from './RawData.js';
import { Sc2Service } from './Sc2Service';
import BitmapDisplay from './BitmapDisplay';

class App extends Component {
  constructor() {
    super();
    this.state = {
      observation: null,
      visibility: null
    };
  }

  async componentDidMount(){
    let service = new Sc2Service();
    const observation = (await service.getObservation()).observation.observation;

    this.setState({
      observation: observation,
      visibility: await service.getBitmap()
    });
  }  

  render() {
    return (
      <div className="App" >
        <header className="App-header">
          HiveMind
      </header>
        <BitmapDisplay src={this.state.visibility} />
        <RawData rawData={this.state.observation} />
      </div>
    );
  }
}

export default App;
