import MapPainter from './MapPainter.js';
import MapPainterConfig from './MapPainterConfig.js';
import Alignment from './Alignment.js';
import TerritoryAreas from './TerritoryAreas.js';
import { downloadFile, fetchImage, fetchJson, promptLoadLocalFile } from './File.js';


const territoryData = await fetchJson('./data/territories.json');
const territoryLookup = {};
for (const territory of territoryData.Territories) {
    territoryLookup[territory.CastleTerritoryId] = territory;
}

const alignment = new Alignment();
const canvas = document.getElementById('canvas');
canvas.width = (territoryData.Max.x / alignment.scaleDown) - alignment.offsetX;
canvas.height = (territoryData.Max.y / alignment.scaleDown);

const backgroundImage = await fetchImage('./images/map-background.png');
const mapPainter = new MapPainter(canvas, backgroundImage, territoryData, alignment);
const painterConfig = new MapPainterConfig();

const jsonOutputEl = document.getElementById('output-json');
let jsonOutput = '';
const updateJsonOutput = () => {
    const output = {};
    for (const [territoryId, territory] of Object.entries(territoryLookup)) {
        output[territoryId] = territory.Score;
    }
    jsonOutput = JSON.stringify(output, null, "  ");
    jsonOutputEl.value = jsonOutput;
};

const update = () => {
    mapPainter.paint(painterConfig);
    updateJsonOutput();
};

const mapContainer = document.getElementById('map-container');
const territoryAreas = new TerritoryAreas(territoryLookup, alignment, mapContainer);
territoryAreas.init();
territoryAreas.onScoreEdited(update);

document.getElementById('checkbox-show-bounding-rectangles').addEventListener('change', (event) => {
    painterConfig.showBoundingRectangles = !!event.currentTarget.checked;
    mapPainter.paint(painterConfig);
});

document.getElementById('checkbox-show-territory-ids').addEventListener('change', (event) => {
    painterConfig.showTerritoryIds = !!event.currentTarget.checked;
    mapPainter.paint(painterConfig);
});

document.getElementById('btn-download-json').addEventListener('click', () => {
    const file = new File([jsonOutput], 'territoryScores.json', {
        type: 'text/plain',
    });
    downloadFile(file);
});

document.getElementById('btn-download-map').addEventListener('click', async function() {
    const imageBlob = await new Promise(resolve => canvas.toBlob(resolve));
    const file = new File([imageBlob], 'territory-map.png', {
        type: 'image/png',
    });
    downloadFile(file);
});

document.getElementById('btn-import-json').addEventListener('click', async function() {
    const scoresById = JSON.parse(await promptLoadLocalFile('.json'));
    for (const [territoryId, score] of Object.entries(scoresById)) {
        territoryLookup[territoryId].Score = score;
    }
    update();  
});

update();
