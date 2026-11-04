const fs = require('fs');
const path = require('path');
const rate = 44100;

function random(seed) {
  let value = seed >>> 0;
  return () => ((value = (value * 1664525 + 1013904223) >>> 0) / 4294967296) * 2 - 1;
}
function make(seconds) { return new Float32Array(Math.floor(seconds * rate)); }
function tone(b, start, length, hz, gain, decay = 4) {
  const a = Math.max(0, Math.floor(start * rate)), z = Math.min(b.length, Math.floor((start + length) * rate));
  for (let i = a; i < z; i++) {
    const t = (i - a) / rate, p = t / length;
    const env = Math.min(1, t / 0.01) * Math.exp(-decay * p);
    b[i] += (Math.sin(Math.PI * 2 * hz * t) + Math.sin(Math.PI * 2 * hz * 2 * t) * 0.18) * gain * env;
  }
}
function noise(b, start, length, gain, seed, lowPass = 0) {
  const r = random(seed); let previous = 0;
  const a = Math.max(0, Math.floor(start * rate)), z = Math.min(b.length, Math.floor((start + length) * rate));
  for (let i = a; i < z; i++) {
    const t = (i - a) / rate, p = t / length;
    const env = Math.min(1, t / 0.02) * Math.min(1, (length - t) / 0.06);
    const n = r(); previous += (n - previous) * lowPass;
    b[i] += (lowPass ? previous : n) * gain * env;
  }
}
function save(file, b) {
  const pcm = Buffer.alloc(b.length * 2); let peak = 0;
  for (const sample of b) peak = Math.max(peak, Math.abs(sample));
  const scale = peak > 0.86 ? 0.86 / peak : 1;
  for (let i = 0; i < b.length; i++) pcm.writeInt16LE(Math.round(Math.max(-1, Math.min(1, b[i] * scale)) * 32767), i * 2);
  const h = Buffer.alloc(44);
  h.write('RIFF', 0); h.writeUInt32LE(36 + pcm.length, 4); h.write('WAVE', 8); h.write('fmt ', 12);
  h.writeUInt32LE(16, 16); h.writeUInt16LE(1, 20); h.writeUInt16LE(1, 22); h.writeUInt32LE(rate, 24);
  h.writeUInt32LE(rate * 2, 28); h.writeUInt16LE(2, 32); h.writeUInt16LE(16, 34); h.write('data', 36); h.writeUInt32LE(pcm.length, 40);
  fs.writeFileSync(file, Buffer.concat([h, pcm]));
}
const out = path.resolve(__dirname, '..', 'Interaction Sound Previews');
fs.mkdirSync(out, { recursive: true });

let b = make(1.4); tone(b, .10, .20, 152, .16, 2); tone(b, .39, .20, 152, .16, 2); tone(b, .72, .22, 880, .12, 7); tone(b, .84, .20, 1175, .08, 8); save(path.join(out, 'Phone_Vibration_Notification.wav'), b);
b = make(1.5); tone(b, .06, .04, 220, .18, 13); tone(b, .31, .10, 920, .13, 7); tone(b, .48, .10, 1160, .10, 7); noise(b, .78, .30, .10, 1, .28); save(path.join(out, 'Meal_Microwave_And_Package.wav'), b);
b = make(1.4); noise(b, .08, .20, .17, 2, .12); noise(b, .38, .18, .14, 3, .14); tone(b, .82, .05, 340, .09, 15); save(path.join(out, 'Tidy_Rustle_And_Object.wav'), b);
b = make(1.3); tone(b, .08, .06, 390, .11, 15); noise(b, .15, .35, .11, 4, .08); tone(b, .59, .05, 620, .08, 17); tone(b, .71, .05, 740, .06, 17); save(path.join(out, 'Medicine_Bottle_And_Pills.wav'), b);
b = make(3.0); noise(b, .05, 2.70, .14, 5, .22); tone(b, .02, .06, 170, .08, 18); tone(b, 2.72, .09, 140, .08, 15); save(path.join(out, 'Shower_Water.wav'), b);
b = make(1.5); tone(b, .12, .70, 690, .055, 2); tone(b, .12, .72, 1035, .028, 3); noise(b, .20, .45, .018, 6, .10); save(path.join(out, 'Mirror_Glass_Tone.wav'), b);
b = make(3.0); noise(b, .05, 2.75, .10, 7, .06); tone(b, .18, 1.6, 117, .025, 1.5); save(path.join(out, 'Window_Distant_Wind.wav'), b);
b = make(1.5); tone(b, .09, .07, 190, .13, 12); tone(b, .26, .22, 95, .14, 5); noise(b, .52, .28, .055, 8, .09); save(path.join(out, 'Door_Handle_And_Hallway.wav'), b);
b = make(1.7); noise(b, .06, .55, .09, 9, .16); tone(b, .77, .35, 196, .045, 5); tone(b, 1.05, .36, 164.81, .035, 5); save(path.join(out, 'Sleep_Bedding_And_Fade.wav'), b);
console.log(out);
