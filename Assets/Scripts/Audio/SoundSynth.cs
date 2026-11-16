using System;
using UnityEngine;

/// <summary>
/// Computes the sound effects of the game instead of importing them, in the manner of the 16-bit consoles:
/// square, triangle and saw waves, filtered noise, two-operator FM for the bells and the metal (Mega Drive), an echo
/// (Super Nintendo), all mixed at 32 kHz and rounded to 16 bits.
/// A sound is a few voices added to a buffer: see Build for the recipe of each of them.
/// </summary>
public static class SoundSynth
{
    public const int SampleRate = 32000;

    private enum Wave { Square, Triangle, Saw }

    private static readonly System.Random random = new System.Random(1987);

    /// <summary>The clip of a sound effect.</summary>
    public static AudioClip Build(Sfx sfx)
    {
        float[] samples = Samples(sfx);
        // the voices added together may be too loud: the sound is then turned down rather than distorted
        float peak = 0f;
        foreach (float sample in samples)
        {
            peak = Mathf.Max(peak, Mathf.Abs(sample));
        }
        float gain = peak > 0.9f ? 0.9f / peak : 1f;
        // the level of a 16-bit sample, as the consoles played them
        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] = Mathf.Round(samples[i] * gain * 32767f) / 32767f;
        }
        AudioClip clip = AudioClip.Create(sfx.ToString(), samples.Length, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static float[] Samples(Sfx sfx)
    {
        float[] b;
        switch (sfx)
        {
            case Sfx.Click:
                b = Buffer(0.06f);
                Tone(b, Wave.Square, 0f, 0.05f, 1250f, 900f, 0.22f, 0.25f);
                return b;

            case Sfx.Tick:
                b = Buffer(0.04f);
                Tone(b, Wave.Triangle, 0f, 0.03f, 1900f, 1700f, 0.3f);
                return b;

            case Sfx.Step:
                b = Buffer(0.08f);
                Tone(b, Wave.Square, 0f, 0.07f, 280f, 620f, 0.2f, 0.25f);
                return b;

            case Sfx.Pop:
                b = Buffer(0.07f);
                Tone(b, Wave.Triangle, 0f, 0.06f, 850f, 1500f, 0.35f);
                return b;

            case Sfx.PanelOpen:
                b = Buffer(0.22f);
                Bell(b, 0f, 0.1f, Note(76), 2f, 1.5f, 0.3f);
                Bell(b, 0.06f, 0.15f, Note(81), 2f, 1.5f, 0.3f);
                return b;

            case Sfx.PanelClose:
                b = Buffer(0.22f);
                Bell(b, 0f, 0.1f, Note(81), 2f, 1.5f, 0.28f);
                Bell(b, 0.06f, 0.15f, Note(76), 2f, 1.5f, 0.28f);
                return b;

            case Sfx.Error:
                b = Buffer(0.3f);
                Tone(b, Wave.Square, 0f, 0.11f, 196f, 185f, 0.3f, 0.3f);
                Tone(b, Wave.Square, 0.14f, 0.15f, 156f, 139f, 0.3f, 0.3f);
                Tone(b, Wave.Triangle, 0f, 0.28f, 78f, 70f, 0.3f);
                return b;

            case Sfx.Coins:
                b = Buffer(0.75f);
                Bell(b, 0f, 0.09f, Note(83), 3f, 2f, 0.3f);
                Bell(b, 0.08f, 0.45f, Note(88), 3f, 2f, 0.32f);
                Tone(b, Wave.Square, 0.08f, 0.2f, Note(88), Note(88), 0.08f, 0.25f);
                Echo(b, 0.12f, 0.3f);
                return b;

            case Sfx.Dig:
                b = Buffer(0.4f);
                // two blows of the pickaxe
                for (int i = 0; i < 2; i++)
                {
                    float at = i * 0.17f;
                    Noise(b, at, 0.1f, 0.4f, 6000f, 700f);
                    Tone(b, Wave.Square, at, 0.09f, 210f, 70f, 0.3f, 0.5f);
                    Bell(b, at, 0.06f, 1400f, 3.7f, 3f, 0.12f);
                }
                return b;

            case Sfx.Hit:
                b = Buffer(0.1f);
                Noise(b, 0f, 0.07f, 0.3f, 5000f, 900f);
                Tone(b, Wave.Square, 0f, 0.08f, 320f, 120f, 0.25f, 0.5f);
                return b;

            case Sfx.CriticalHit:
                b = Buffer(0.22f);
                Noise(b, 0f, 0.1f, 0.4f, 8000f, 800f);
                Tone(b, Wave.Square, 0f, 0.12f, 420f, 90f, 0.3f, 0.5f);
                Bell(b, 0.02f, 0.18f, 1800f, 3.7f, 3f, 0.2f);
                return b;

            case Sfx.Hurt:
                b = Buffer(0.12f);
                Tone(b, Wave.Triangle, 0f, 0.1f, 200f, 80f, 0.45f);
                Noise(b, 0f, 0.06f, 0.15f, 1800f, 400f);
                return b;

            case Sfx.SkillUse:
                b = Buffer(0.25f);
                Tone(b, Wave.Square, 0f, 0.1f, 500f, 1100f, 0.18f, 0.25f);
                Bell(b, 0.06f, 0.18f, 1500f, 2f, 2f, 0.25f);
                return b;

            case Sfx.MonsterDown:
                b = Buffer(0.35f);
                Tone(b, Wave.Saw, 0f, 0.3f, 400f, 50f, 0.3f);
                Noise(b, 0.05f, 0.25f, 0.2f, 3000f, 300f);
                return b;

            case Sfx.BarricadeBreak:
                b = Buffer(0.4f);
                Noise(b, 0f, 0.3f, 0.45f, 4000f, 250f);
                Tone(b, Wave.Square, 0f, 0.15f, 160f, 50f, 0.35f, 0.5f);
                Tone(b, Wave.Square, 0.1f, 0.2f, 120f, 40f, 0.25f, 0.5f);
                return b;

            case Sfx.Defeat:
                b = Buffer(0.9f);
                Tone(b, Wave.Triangle, 0f, 0.25f, 330f, 300f, 0.4f);
                Tone(b, Wave.Triangle, 0.25f, 0.25f, 262f, 240f, 0.4f);
                Tone(b, Wave.Triangle, 0.5f, 0.4f, 196f, 110f, 0.45f);
                return b;

            case Sfx.DigNothing:
                b = Buffer(0.25f);
                Tone(b, Wave.Triangle, 0f, 0.2f, 150f, 60f, 0.5f);
                Noise(b, 0f, 0.12f, 0.2f, 1500f, 300f);
                return b;

            case Sfx.DigFound:
                b = Buffer(0.7f);
                Arpeggio(b, 0f, 0.055f, 0.25f, 0.26f, 84, 88, 91, 96);
                Noise(b, 0f, 0.25f, 0.06f, 9000f, 12000f);
                Echo(b, 0.11f, 0.35f);
                return b;

            case Sfx.ExpeditionSend:
                b = Buffer(1.0f);
                // a little march: the crew leaves
                Melody(b, Wave.Square, 0f, 0.09f, 0.22f, 0.25f, 72, 72, 76, 79, 84);
                Tone(b, Wave.Triangle, 0f, 0.18f, Note(48), Note(48), 0.35f);
                Tone(b, Wave.Triangle, 0.18f, 0.18f, Note(55), Note(55), 0.35f);
                Tone(b, Wave.Triangle, 0.36f, 0.3f, Note(60), Note(60), 0.35f);
                Noise(b, 0f, 0.05f, 0.15f, 8000f, 4000f);
                Noise(b, 0.18f, 0.05f, 0.15f, 8000f, 4000f);
                Echo(b, 0.15f, 0.3f);
                return b;

            case Sfx.ExpeditionReturn:
                b = Buffer(1.1f);
                Arpeggio(b, 0f, 0.1f, 0.4f, 0.28f, 79, 84, 88, 91);
                Tone(b, Wave.Triangle, 0f, 0.5f, Note(60), Note(60), 0.25f);
                Echo(b, 0.16f, 0.35f);
                return b;

            case Sfx.EventReach:
                b = Buffer(0.2f);
                Tone(b, Wave.Triangle, 0f, 0.07f, 880f, 880f, 0.3f);
                Tone(b, Wave.Triangle, 0.07f, 0.1f, 1320f, 1320f, 0.3f);
                return b;

            case Sfx.ChestOpen:
                b = Buffer(1.1f);
                // the lid creaks, then the light comes out
                Tone(b, Wave.Saw, 0f, 0.3f, 85f, 150f, 0.22f, 0.5f, 22f, 0.06f);
                Noise(b, 0.28f, 0.08f, 0.3f, 3000f, 800f);
                Arpeggio(b, 0.33f, 0.05f, 0.3f, 0.24f, 72, 76, 79, 84, 88, 91);
                Noise(b, 0.33f, 0.4f, 0.05f, 9000f, 13000f);
                Echo(b, 0.13f, 0.35f);
                return b;

            case Sfx.ItemsFound:
                b = Buffer(0.9f);
                Melody(b, Wave.Square, 0f, 0.08f, 0.2f, 0.25f, 76, 79, 84);
                Bell(b, 0.24f, 0.4f, Note(88), 2f, 2f, 0.28f);
                Tone(b, Wave.Triangle, 0f, 0.5f, Note(55), Note(60), 0.3f);
                Echo(b, 0.14f, 0.3f);
                return b;

            case Sfx.RareItem:
                b = Buffer(1.6f);
                Melody(b, Wave.Square, 0f, 0.1f, 0.2f, 0.25f, 72, 76, 79, 84, 88);
                Chord(b, 0.5f, 0.7f, 0.16f, 72, 76, 79, 84);
                Arpeggio(b, 0.5f, 0.06f, 0.3f, 0.2f, 96, 100, 103, 108);
                Tone(b, Wave.Triangle, 0f, 1.2f, Note(48), Note(48), 0.3f);
                Echo(b, 0.17f, 0.4f);
                return b;

            case Sfx.Equip:
                b = Buffer(0.35f);
                // metal on metal: an FM voice whose partials are not harmonic
                Bell(b, 0f, 0.25f, 620f, 3.53f, 4f, 0.32f);
                Bell(b, 0.03f, 0.2f, 930f, 2.41f, 3f, 0.2f);
                Noise(b, 0f, 0.03f, 0.3f, 9000f, 3000f);
                return b;

            case Sfx.Unequip:
                b = Buffer(0.25f);
                Tone(b, Wave.Triangle, 0f, 0.14f, 560f, 280f, 0.4f);
                Noise(b, 0f, 0.1f, 0.15f, 2500f, 600f);
                return b;

            case Sfx.PortalOpen:
                b = Buffer(1.1f);
                // a breath that rises
                Noise(b, 0f, 1f, 0.3f, 300f, 7000f, true);
                Tone(b, Wave.Saw, 0f, 1f, 70f, 420f, 0.18f, 0.5f, 9f, 0.03f, true);
                Tone(b, Wave.Triangle, 0f, 1f, 140f, 840f, 0.2f, 0.5f, 0f, 0f, true);
                Echo(b, 0.09f, 0.3f);
                return b;

            case Sfx.SummonStep:
                b = Buffer(0.6f);
                // the portal climbs a rank: played higher at each step
                Chord(b, 0f, 0.35f, 0.14f, 60, 67, 72);
                Bell(b, 0f, 0.4f, Note(84), 2f, 3f, 0.25f);
                Tone(b, Wave.Triangle, 0f, 0.2f, 110f, 55f, 0.4f);
                Noise(b, 0f, 0.08f, 0.2f, 7000f, 2000f);
                Echo(b, 0.11f, 0.3f);
                return b;

            case Sfx.SummonBurst:
                b = Buffer(1.4f);
                Noise(b, 0f, 1.1f, 0.55f, 9000f, 250f);
                Tone(b, Wave.Triangle, 0f, 0.8f, 130f, 28f, 0.6f);
                Tone(b, Wave.Square, 0f, 0.25f, 900f, 90f, 0.2f, 0.5f);
                Echo(b, 0.14f, 0.4f);
                return b;

            case Sfx.RankStamp:
                b = Buffer(0.25f);
                Tone(b, Wave.Triangle, 0f, 0.18f, 210f, 45f, 0.6f);
                Noise(b, 0f, 0.05f, 0.35f, 5000f, 900f);
                return b;

            case Sfx.RevealCommon:
                b = Buffer(0.5f);
                Melody(b, Wave.Square, 0f, 0.09f, 0.18f, 0.25f, 72, 79);
                Tone(b, Wave.Triangle, 0f, 0.3f, Note(48), Note(48), 0.25f);
                return b;

            case Sfx.RevealRare:
                b = Buffer(1.0f);
                Arpeggio(b, 0f, 0.08f, 0.35f, 0.26f, 72, 76, 79, 84);
                Tone(b, Wave.Square, 0.24f, 0.3f, Note(84), Note(84), 0.1f, 0.25f, 6f, 0.01f);
                Tone(b, Wave.Triangle, 0f, 0.6f, Note(48), Note(48), 0.3f);
                Echo(b, 0.15f, 0.35f);
                return b;

            case Sfx.RevealEpic:
                b = Buffer(1.8f);
                Melody(b, Wave.Square, 0f, 0.1f, 0.2f, 0.25f, 67, 72, 76, 79, 84, 88);
                Chord(b, 0.6f, 0.8f, 0.15f, 72, 76, 79, 84);
                Arpeggio(b, 0.6f, 0.07f, 0.35f, 0.22f, 91, 96, 100, 103);
                Tone(b, Wave.Triangle, 0f, 0.6f, Note(43), Note(43), 0.32f);
                Tone(b, Wave.Triangle, 0.6f, 0.8f, Note(48), Note(48), 0.32f);
                Noise(b, 0.6f, 0.5f, 0.08f, 8000f, 12000f);
                Echo(b, 0.17f, 0.4f);
                return b;

            case Sfx.RevealLegend:
                b = Buffer(3.2f);
                // the whole fanfare: a run up two octaves, three chords, and bells over them
                Melody(b, Wave.Square, 0f, 0.075f, 0.2f, 0.25f, 60, 64, 67, 72, 76, 79, 84, 88, 91, 96);
                Chord(b, 0.75f, 0.45f, 0.14f, 65, 69, 72, 77);
                Chord(b, 1.2f, 0.45f, 0.14f, 67, 71, 74, 79);
                Chord(b, 1.65f, 1.2f, 0.16f, 72, 76, 79, 84, 88);
                Arpeggio(b, 0.75f, 0.075f, 0.3f, 0.2f, 89, 93, 96, 101, 91, 95, 98, 103);
                Arpeggio(b, 1.65f, 0.07f, 0.5f, 0.22f, 96, 100, 103, 108, 112, 108, 103, 108);
                Tone(b, Wave.Triangle, 0.75f, 0.45f, Note(41), Note(41), 0.34f);
                Tone(b, Wave.Triangle, 1.2f, 0.45f, Note(43), Note(43), 0.34f);
                Tone(b, Wave.Triangle, 1.65f, 1.2f, Note(48), Note(48), 0.36f);
                Noise(b, 1.65f, 1.2f, 0.08f, 7000f, 13000f);
                Echo(b, 0.19f, 0.45f);
                return b;

            case Sfx.GameStart:
                b = Buffer(1.0f);
                Melody(b, Wave.Square, 0f, 0.08f, 0.2f, 0.25f, 72, 76, 79, 84, 79, 84);
                Bell(b, 0.4f, 0.45f, Note(96), 2f, 2f, 0.2f);
                Tone(b, Wave.Triangle, 0f, 0.55f, Note(48), Note(60), 0.3f);
                Echo(b, 0.14f, 0.3f);
                return b;

            default:
                return Buffer(0.02f);
        }
    }

    // ---- voices

    private static float[] Buffer(float seconds)
    {
        return new float[Mathf.CeilToInt(seconds * SampleRate)];
    }

    /// <summary>The frequency of a MIDI note: 69 is the A of 440 Hz, 72 the C above.</summary>
    private static float Note(int midi)
    {
        return 440f * Mathf.Pow(2f, (midi - 69) / 12f);
    }

    /// <summary>The loudness of a voice over its life (0..1): a short attack then a decay, or a swell.</summary>
    private static float Envelope(float life, float duration, bool swell)
    {
        float fadeOut = Mathf.Clamp01((1f - life) * duration / 0.01f);
        if (swell) return life * life * fadeOut;
        float attack = Mathf.Clamp01(life * duration / 0.004f);
        return attack * Mathf.Exp(-3.5f * life) * fadeOut;
    }

    /// <summary>A wave sliding from a frequency to another.</summary>
    private static void Tone(float[] buffer, Wave wave, float start, float duration, float from, float to, float volume,
        float duty = 0.5f, float vibratoRate = 0f, float vibratoDepth = 0f, bool swell = false)
    {
        int first = Mathf.RoundToInt(start * SampleRate);
        int count = Mathf.RoundToInt(duration * SampleRate);
        double phase = 0;
        for (int i = 0; i < count && first + i < buffer.Length; i++)
        {
            float life = i / (float)count;
            float frequency = from * Mathf.Pow(to / from, life);
            if (vibratoRate > 0f) frequency *= 1f + vibratoDepth * Mathf.Sin(2f * Mathf.PI * vibratoRate * i / SampleRate);
            phase += frequency / SampleRate;
            float p = (float)(phase - Math.Floor(phase));
            float value;
            switch (wave)
            {
                case Wave.Square: value = p < duty ? 1f : -1f; break;
                case Wave.Saw: value = 2f * p - 1f; break;
                default: value = 4f * Mathf.Abs(p - 0.5f) - 1f; break;
            }
            buffer[first + i] += value * volume * Envelope(life, duration, swell);
        }
    }

    /// <summary>
    /// A two-operator FM voice: a sine whose phase is moved by another sine "ratio" times higher. The modulation
    /// fades with the note, which gives the bright attack of a bell; a ratio that is not a whole number sounds like metal.
    /// </summary>
    private static void Bell(float[] buffer, float start, float duration, float frequency, float ratio, float index, float volume)
    {
        int first = Mathf.RoundToInt(start * SampleRate);
        int count = Mathf.RoundToInt(duration * SampleRate);
        for (int i = 0; i < count && first + i < buffer.Length; i++)
        {
            float life = i / (float)count;
            float time = i / (float)SampleRate;
            float envelope = Envelope(life, duration, false);
            float modulator = Mathf.Sin(2f * Mathf.PI * frequency * ratio * time) * index * (1f - life);
            buffer[first + i] += Mathf.Sin(2f * Mathf.PI * frequency * time + modulator) * volume * envelope;
        }
    }

    /// <summary>White noise through a low-pass filter whose cutoff slides: blows, breaths and shimmers.</summary>
    private static void Noise(float[] buffer, float start, float duration, float volume, float cutoffFrom, float cutoffTo,
        bool swell = false)
    {
        int first = Mathf.RoundToInt(start * SampleRate);
        int count = Mathf.RoundToInt(duration * SampleRate);
        float filtered = 0f;
        for (int i = 0; i < count && first + i < buffer.Length; i++)
        {
            float life = i / (float)count;
            float cutoff = cutoffFrom * Mathf.Pow(cutoffTo / cutoffFrom, life);
            float amount = Mathf.Clamp01(2f * Mathf.PI * cutoff / SampleRate);
            filtered += amount * ((float)random.NextDouble() * 2f - 1f - filtered);
            buffer[first + i] += filtered * volume * Envelope(life, duration, swell);
        }
    }

    /// <summary>Notes played one after the other with a wave.</summary>
    private static void Melody(float[] buffer, Wave wave, float start, float step, float noteDuration, float volume, params int[] notes)
    {
        for (int i = 0; i < notes.Length; i++)
        {
            float frequency = Note(notes[i]);
            Tone(buffer, wave, start + i * step, noteDuration, frequency, frequency, volume * 0.8f, 0.25f);
        }
    }

    /// <summary>Notes played one after the other with bells.</summary>
    private static void Arpeggio(float[] buffer, float start, float step, float noteDuration, float volume, params int[] notes)
    {
        for (int i = 0; i < notes.Length; i++)
        {
            Bell(buffer, start + i * step, noteDuration, Note(notes[i]), 2f, 2f, volume);
        }
    }

    /// <summary>Notes held together, with a slow vibrato.</summary>
    private static void Chord(float[] buffer, float start, float duration, float volume, params int[] notes)
    {
        foreach (int note in notes)
        {
            float frequency = Note(note);
            Tone(buffer, Wave.Square, start, duration, frequency, frequency, volume, 0.5f, 5.5f, 0.006f);
        }
    }

    /// <summary>Repeats the sound, quieter each time, as the echo of the Super Nintendo did.</summary>
    private static void Echo(float[] buffer, float delay, float feedback)
    {
        int offset = Mathf.RoundToInt(delay * SampleRate);
        for (int i = offset; i < buffer.Length; i++)
        {
            buffer[i] += buffer[i - offset] * feedback;
        }
    }
}
