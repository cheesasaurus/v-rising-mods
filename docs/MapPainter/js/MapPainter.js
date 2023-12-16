export default class MapPainter {
    canvas;
    backgroundImage;
    alignment;

    constructor(canvas, backgroundImage, territoryData, alignment) {
        this.canvas = canvas;
        this.backgroundImage = backgroundImage;
        this.territoryData = territoryData;
        this.alignment = alignment;
    }

    async paint(config) {
        const canvas = this.canvas;
        const data = this.territoryData;

        const scaleDown = this.alignment.scaleDown;
        const offsetX = this.alignment.offsetX;
        const offsetY = this.alignment.offsetY;

        const ctx = canvas.getContext("2d");
        ctx.drawImage(this.backgroundImage, 0, 0);

        for (const territory of data.Territories) {
            const min = territory.BoundingRectangle.Min;
            const max = territory.BoundingRectangle.Max;
            const width = (max.x - min.x) / scaleDown;
            const height = (max.y - min.y) / scaleDown;

            // the game y axis goes from bottom to top,
            // but the canvas y axis goes from top to bottom,
            const x = (min.x / scaleDown) + offsetX;
            const y = ctx.canvas.height - (max.y / scaleDown) + offsetY;

            // bounding rectangle
            if (config.showBoundingRectangles) {
                ctx.strokeStyle = "magenta";
                ctx.lineWidth = 1;
                ctx.beginPath();
                ctx.rect(x, y, width, height);
                ctx.stroke();
            }

            const centerX = x + (width / 2);
            const centerY = y + (height / 2);

            // score
            const scoreText = territory.Score;
            let scoreY = centerY;
            if (config.showTerritoryIds) {
                scoreY -= 5;
            }
            ctx.font = "bold 26px Roboto";
            ctx.textAlign = "center";
            ctx.textBaseline = "middle";
            ctx.fillStyle = "white";
            ctx.strokeStyle = "black";
            ctx.lineWidth = 2;
            ctx.strokeText(scoreText, centerX, scoreY);
            ctx.fillText(scoreText, centerX, scoreY);

            // territory id
            if (config.showTerritoryIds) {
                const idText = '#' + territory.CastleTerritoryId;
                const idY = centerY + 15;
                ctx.font = "12px Arial";
                ctx.textAlign = "center";
                ctx.textBaseline = "middle";
                ctx.fillStyle = "white";
                ctx.strokeStyle = "black";
                ctx.lineWidth = 1;
                ctx.strokeText(idText, centerX, idY);
                ctx.fillText(idText, centerX, idY);
            }
        }
    }

}
