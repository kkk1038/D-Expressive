const fs = require('fs');
const path = require('path');

const sampleRate = 44100;
const duration = 36;
const count = sampleRate * duration;
const tempo = 64;
const beat = 60 / tempo;

function seededRandom(seed) {
  let value = seed >>> 0;
  return () => {
    value = (value * 1664525 + 1013904223) >>> 0;
    return value / 4294967296;
  };
}

function addTone(buffer, start, length, frequency, volume, kind = 'bell') {
  const first = Math.max(0, Math.floor(start * sampleRate));
  const last = Math.min(buffer.length, Math.floor((start + length) * sampleRate));
  for (let i = first; i < last; i++) {
    const t = (i - first) / sampleRate;
    const progress = t / length;
    const envelope = kind === 'pad'
      ? Math.min(1, t / 0.45) * Math.min(1, (length - t) / 0.8) * 0.45
      : Math.min(1, t / 0.025) * Math.exp(-3.1 * progress);
    const body = kind === 'pad'
      ? Math.sin(2 * Math.PI * frequency * t) + 0.22 * Math.sin(2 * Math.PI * frequency * 2 * t)
      : Math.sin(2 * Math.PI * frequency * t) + 0.32 * Math.sin(2 * Math.PI * frequency * 2 * t) + 0.12 * Math.sin(2 * Math.PI * frequency * 3 * t);
    buffer[i] += body * volume * envelope;
  }
}

function createBase() {
  const buffer = new Float32Array(count);
  // Slow E-minor harmony and a descending melody leave more empty space in the room.
  const chords = [[82.41, 123.47, 164.81], [73.42, 110.00, 146.83], [65.41, 98.00, 130.81], [61.74, 92.50, 123.47]];
  const melody = [659.25, 622.25, 587.33, 523.25, 493.88, 440.00, 493.88, 523.25, 587.33, 523.25, 493.88, 440.00, 392.00, 440.00, 493.88, 440.00];
  const bar = beat * 4;
  for (let start = 0, barIndex = 0; start < duration; start += bar, barIndex++) {
    for (const note of chords[barIndex % chords.length]) addTone(buffer, start, bar, note, 0.15, 'pad');
    for (let step = 0; step < 4; step++) {
      const note = melody[(barIndex * 4 + step) % melody.length];
      addTone(buffer, start + step * beat, beat * 1.25, note, 0.09);
      if (step === 0) addTone(buffer, start + step * beat, beat * 0.8, note / 2, 0.035, 'bell');
    }
  }
  return buffer;
}

function writeWave(fileName, source, state) {
  const random = seededRandom(state === 'low' ? 23 : 11);
  const pcm = Buffer.alloc(count * 2);
  for (let i = 0; i < count; i++) {
    const t = i / sampleRate;
    let sample = source[i];
    if (state === 'mid') {
      // Brief, rare dropouts and a low static layer below mental state 50.
      const position = t % 6;
      const gap = position > 4.68 && position < 4.86;
      const staticAmount = gap ? 0.11 : (position > 4.3 ? 0.025 : 0);
      sample = gap ? source[i] * 0.08 : sample;
      sample += (random() * 2 - 1) * staticAmount;
    } else if (state === 'low') {
      // Long, frequent silence with rough static: the same melody is only intermittently reachable.
      const position = t % 5.5;
      const audible = position < 1.55 || (position > 3.65 && position < 4.05);
      const fade = audible ? Math.min(1, Math.min(position / 0.12, (1.55 - position) / 0.12)) : 0;
      sample = source[i] * Math.max(0, fade) * 0.40;
      sample += (random() * 2 - 1) * (audible ? 0.10 : 0.18);
    }
    sample = Math.tanh(sample * 1.25);
    pcm.writeInt16LE(Math.round(Math.max(-1, Math.min(1, sample)) * 32767), i * 2);
  }
  const header = Buffer.alloc(44);
  header.write('RIFF', 0); header.writeUInt32LE(36 + pcm.length, 4); header.write('WAVE', 8);
  header.write('fmt ', 12); header.writeUInt32LE(16, 16); header.writeUInt16LE(1, 20);
  header.writeUInt16LE(1, 22); header.writeUInt32LE(sampleRate, 24);
  header.writeUInt32LE(sampleRate * 2, 28); header.writeUInt16LE(2, 32); header.writeUInt16LE(16, 34);
  header.write('data', 36); header.writeUInt32LE(pcm.length, 40);
  fs.writeFileSync(fileName, Buffer.concat([header, pcm]));
}

const output = path.resolve(__dirname, '..', 'Audio Previews');
fs.mkdirSync(output, { recursive: true });
const base = createBase();
writeWave(path.join(output, 'Melancholic_Room_Normal.wav'), base, 'normal');
writeWave(path.join(output, 'Melancholic_Room_Mental_50.wav'), base, 'mid');
writeWave(path.join(output, 'Melancholic_Room_Mental_10.wav'), base, 'low');
console.log(output);
