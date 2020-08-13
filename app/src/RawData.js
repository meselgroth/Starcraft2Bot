import React from 'react';
import ReactJson from 'react-json-view'

export const RawData = ({observation, gameinfo, gameplay}) => (
    <div>
        <h1>Raw Data</h1>
        <h2>Observation</h2>
        Snapshot of current game state represented as structured data.
        <ReactJson src={observation} />
        <h2>Gameplay</h2>
        Static information about gameplay elements.
        <ReactJson src={gameplay} />
        <h2>Game Info</h2>
        Static information about the map.
        <ReactJson src={gameinfo} />
    </div>
);