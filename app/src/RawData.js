import React from 'react';
import ReactJson from 'react-json-view'

export const RawData = ({rawData}) => (
    <div>
        <h1>Raw observation Data</h1>
        <ReactJson src={rawData} />
    </div>
);