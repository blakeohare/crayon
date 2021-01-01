const NoriAudio = (() => {

    let audioContext = null;
    let acCbQueue = [];
    let interactionOccurred = () => {
        if (audioContext === null) {
            audioContext = new (window.AudioContext || window.webkitAudioContext)();
            platformSpecificHandleEvent(-1, 'audio', 'ready');
            for (let cb of acCbQueue) {
                cb(audioContext);
            }
            acCbQueue = [];
        }
    };

    let getAudioContext = cb => {
        if (audioContext === null) {
            acCbQueue.push(cb);
        } else {
            setTimeout(() => cb(audioContext), 0);
        }
    };

    let soundById = {};

    let loadSamples = (id, data) => {
        let i = 0;
        let freq = parseInt(data[i++]);
        let channelCount = parseInt(data[i++]);
        let length = parseInt(data[i++]);
        let channels = [];
        for (let j = 0; j < channelCount; ++j) {
            let channel = [];
            for (let k = 0; k < length; ++k) {
                channel.push(parseInt(data[i++]));
            }
            channels.push(channel);
        }
        
        getAudioContext(ctx => {
            let buffer = ctx.createBuffer(channelCount, length, freq);
            let source = ctx.createBufferSource();
            source.buffer = buffer;
            for (let i = 0; i < channels.length; ++i) {
                let channel = channels[i];
                let channelBuffer = buffer.getChannelData(i);
                let len = channel.length;
                for (let j = 0; j < len; ++j) {
                    channelBuffer[j] = channel[j] / 32768.0;
                }
            }
            source.connect(ctx.destination);
            
            soundById[id] = {
                id,
                type: 'samples',
                source,
            };

            platformSpecificHandleEvent(id, 'audio', 'loaded');
        });
    };

    let play = (id, isLoop, fadeDuration) => {
        let snd = soundById[id];
        if (snd.type === 'samples') {
            snd.source.start();
        } else {
            throw new Error("Not implemented");
        }
    };

    let handleAudioEvent = (id, cmd, items, index) => {
        switch (cmd) {
            case 'SAMPLES':
                loadSamples(id, items[index].split(','));
                return 1;

            case 'PLAY':
                let isLoop = parseInt(items[index]) === 1;
                let fadeDuration = parseFloat(items[index + 1]);
                play(id, isLoop, fadeDuration);
                return 2;

            default: throw new Error("Unknown audio command: " + cmd);
        }
    };
    
	return {
        handleAudioEvent,
        interactionOccurred,
        play,
	};
})();
