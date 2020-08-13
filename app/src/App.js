import React, { Component } from 'react';
import './App.css';
import { RawData } from './RawData.js';
import { Sc2Service } from './Sc2Service';
import BitmapDisplay from './BitmapDisplay';

class App extends Component {
  constructor() {
    super();
    this.state = {
      observation: {},
      gameplay: {},
      gameinfo: {},
      visibility: null
    };
  }

  async componentDidMount(){
    let service = new Sc2Service();
    const observation = (await service.getJson('rawdata/observation')).observation;

    this.setState({
      observation: observation,
      gameplay: await service.getJson('rawdata/gameplay'),
      gameinfo: await service.getJson('rawdata/gameinfo'),
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
        <RawData observation={this.state.observation} gameplay={this.state.gameplay} gameinfo={this.state.gameinfo} />
      </div>
    );
  }
}

export default App;
