import React, { Component } from 'react';
import './App.css';
import { RawData } from './RawData.js';
import { Sc2Service } from './Sc2Service';

class App extends Component {
  constructor() {
    super();
    this.state = {
      observation: null
    };
  }

  async componentDidMount(){
    let service = new Sc2Service();
    const observation = await service.getObservation();

    this.setState({
      observation: JSON.stringify(observation)
    });
  }  

  render() {
    return (
      <div className="App" >
        <header className="App-header">
          HiveMind
      </header>
        <RawData rawData={this.state.observation} />
      </div>
    );
  }
}

export default App;
