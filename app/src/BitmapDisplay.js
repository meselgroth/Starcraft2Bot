import React from 'react';

export default class BitmapDisplay extends React.Component {

    async componentDidUpdate() {
        // if (!this.props.src) {
        //     return;
        // }
        // await this.decodeUtf8Bitmap();
    }

    render() {
        return (
            <div>
                {/* <canvas ref="canvas" /> */}
                <span><label className="pic-label">PlacementGrid</label><img src="api/bitmap/PlacementGrid" alt="bitmap" /></span>
                <span><label className="pic-label">PathingGrid</label><img src="api/bitmap/PathingGrid" alt="bitmap" /></span>
                <iframe src="https://localhost:5001/api/bitmap/playablearea" title="PlayableArea"/>
            </div>
        );
    }
    
    async decodeUtf8Bitmap() {
        // try TextDecoder !
        let byteCharacters = this.props.src; // UTF8 encoded string
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
}