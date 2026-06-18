window.lsGet = (key) => localStorage.getItem(key);
window.lsSet = (key, value) => localStorage.setItem(key, value);
window.lsRemove = (key) => localStorage.removeItem(key);

window.compressImage = (dataUrl, maxWidth, maxHeight, quality) =>
    new Promise((resolve) => {
        const img = new Image();
        img.onload = () => {
            let w = img.width, h = img.height;
            const ratio = Math.min(maxWidth / w, maxHeight / h, 1);
            w = Math.round(w * ratio);
            h = Math.round(h * ratio);
            const canvas = document.createElement('canvas');
            canvas.width = w;
            canvas.height = h;
            canvas.getContext('2d').drawImage(img, 0, 0, w, h);
            resolve(canvas.toDataURL('image/jpeg', quality));
        };
        img.onerror = () => resolve(dataUrl);
        img.src = dataUrl;
    });

window.printPage = () => window.print();

window.scrollToTop = () => window.scrollTo({ top: 0, behavior: 'smooth' });
