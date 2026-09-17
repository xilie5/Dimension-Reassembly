using System.Collections.Generic;
using UnityEngine;

namespace CompoundBox
{
    public enum AudioCue
    {
        Move,
        Push,
        Blocked,
        Split,
        Recombine,
        Portal,
        Complete,
        Ui
    }

    public sealed class ProceduralAudio
    {
        private readonly AudioSource source;
        private readonly Dictionary<AudioCue, AudioClip> clips = new Dictionary<AudioCue, AudioClip>();

        public ProceduralAudio(GameObject owner)
        {
            source = owner.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = 0.45f;

            clips[AudioCue.Move] = CreateSweep("Move", 180f, 210f, 0.045f, 0.22f);
            clips[AudioCue.Push] = CreateSweep("Push", 130f, 95f, 0.085f, 0.34f);
            clips[AudioCue.Blocked] = CreateSweep("Blocked", 90f, 60f, 0.05f, 0.18f);
            clips[AudioCue.Split] = CreateSweep("Split", 320f, 640f, 0.12f, 0.36f);
            clips[AudioCue.Recombine] = CreateSweep("Recombine", 460f, 250f, 0.16f, 0.4f);
            clips[AudioCue.Portal] = CreateSweep("Portal", 210f, 880f, 0.2f, 0.42f);
            clips[AudioCue.Complete] = CreateSweep("Complete", 360f, 920f, 0.48f, 0.55f);
            clips[AudioCue.Ui] = CreateSweep("Ui", 520f, 560f, 0.035f, 0.15f);
        }

        public bool Muted { get; set; }

        public void Play(AudioCue cue)
        {
            if (Muted || !clips.TryGetValue(cue, out var clip))
            {
                return;
            }

            source.PlayOneShot(clip);
        }

        private static AudioClip CreateSweep(string name, float startFrequency, float endFrequency, float duration, float gain)
        {
            const int sampleRate = 44100;
            var sampleCount = Mathf.Max(1, Mathf.RoundToInt(sampleRate * duration));
            var samples = new float[sampleCount];
            var phase = 0f;
            for (var i = 0; i < sampleCount; i++)
            {
                var t = i / (float)sampleCount;
                var frequency = Mathf.Lerp(startFrequency, endFrequency, t);
                phase += 2f * Mathf.PI * frequency / sampleRate;
                var envelope = Mathf.Pow(1f - t, 2.6f);
                var overtone = Mathf.Sin(phase * 2f) * 0.14f;
                samples[i] = (Mathf.Sin(phase) + overtone) * envelope * gain;
            }

            var clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
