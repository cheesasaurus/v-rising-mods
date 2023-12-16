

export const downloadFile = file => {
    const link = document.createElement('a')
    const url = URL.createObjectURL(file);
    link.href = url;
    link.download = file.name;
    link.click();
    window.URL.revokeObjectURL(url)
};


export const promptLoadLocalFile = async function(type = 'text/plain') {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = type;

    return new Promise(resolve => {
        input.onchange = e => { 
            const file = e.target.files[0];
            const reader = new FileReader();
            reader.readAsText(file, 'UTF-8');
            reader.onload = readerEvent => {
                resolve(readerEvent.target.result);
            }
        }
        input.click();
    });
};


export const fetchImage = async function(url) {
    const image = new Image();
    await new Promise(resolve => {
        image.onload = resolve;
        image.src = url;
    });
    return image;
};


export const fetchJson = async function(url) {
    const response = await fetch(url);
    return response.json();
};

