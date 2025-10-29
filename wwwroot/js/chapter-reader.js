window.setupKeyboardShortcuts = (dotNetRef) => {
    let isSetup = false;
    
    if (isSetup) return;
    isSetup = true;
    
    document.addEventListener('keydown', async (event) => {
        // Don't trigger shortcuts when typing in input fields
        if (event.target.tagName === 'INPUT' || event.target.tagName === 'TEXTAREA' || event.target.contentEditable === 'true') {
            return;
        }
        
        let key = '';
        
        switch (event.key) {
            case 'ArrowLeft':
                key = 'arrowleft';
                break;
            case 'ArrowRight':
                key = 'arrowright';
                break;
            case ' ':
                event.preventDefault(); // Prevent page scroll
                key = ' ';
                break;
            case 'f':
            case 'F':
                key = 'f';
                break;
            case 's':
            case 'S':
                key = 's';
                break;
            case 'b':
            case 'B':
                key = 'b';
                break;
            case 't':
            case 'T':
                key = 't';
                break;
            case 'Escape':
                key = 'escape';
                break;
            default:
                return;
        }
        
        try {
            await dotNetRef.invokeMethodAsync('HandleKeyPress', key);
        } catch (error) {
            console.error('Error handling key press:', error);
        }
    });
};

// Fullscreen functionality
window.toggleFullscreen = () => {
    if (!document.fullscreenElement) {
        document.documentElement.requestFullscreen().catch(err => {
            console.log(`Error attempting to enable fullscreen: ${err.message}`);
        });
    } else {
        document.exitFullscreen();
    }
};

// Auto scroll functionality
window.startAutoScroll = (speed) => {
    const scrollSpeed = speed || 1;
    const scrollInterval = setInterval(() => {
        window.scrollBy(0, scrollSpeed);
    }, 50);
    
    return scrollInterval;
};

window.stopAutoScroll = (intervalId) => {
    if (intervalId) {
        clearInterval(intervalId);
    }
};
