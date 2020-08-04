import React from 'react';

export default class BitmapDisplay extends React.Component {

    async componentDidUpdate() {
        if (!this.props.src) {
            return;
        }
        // try TextDecoder !
        let byteCharacters = this.props.src;  // UTF8 encoded string
        const byteNumbers = new Array(byteCharacters.length);
        for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        const byteArray = new Uint8Array(byteNumbers);
        const blob = new Blob([byteArray], { type: 'image/bmp' });
        let imageBitmap = await createImageBitmap(blob);
        let canvasCtx = this.refs.canvas.getContext('2d');
        canvasCtx.drawImage(imageBitmap, 0, 0);
    }
    render() {
        return (
            <div>
                <canvas ref="canvas" />
                <img src="api/bitmap" alt="bitmap" />
            </div>
        );
    }
}