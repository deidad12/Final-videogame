using UnityEngine;

// Genera clips de música de fondo de forma 100% procedural (sin depender de archivos de audio
// externos), para asegurar que el juego SIEMPRE tenga una canción de fondo aunque no se haya
// importado ningún AudioClip manualmente en el Inspector.
public static class MusicGenerator
{
    private const int SAMPLE_RATE = 44100;

    // Melodía alegre tipo "chiptune" pastel para el menú y niveles normales.
    public static AudioClip GenerarMusicaFondo()
    {
        float[] notas = { 261.63f, 293.66f, 329.63f, 392.00f, 440.00f, 523.25f };
        int[] patron = { 0, 2, 4, 3, 1, 4, 5, 3, 0, 4, 2, 5, 1, 3, 4, 0 };
        return GenerarClip("MusicaFondoGenerada", notas, patron, 0.28f, 0.28f, true);
    }

    // Melodía más tensa/rápida para la pelea contra el jefe.
    public static AudioClip GenerarMusicaBoss()
    {
        float[] notas = { 220.00f, 246.94f, 261.63f, 293.66f, 329.63f, 349.23f };
        int[] patron = { 0, 0, 3, 2, 0, 0, 4, 3, 5, 5, 2, 1, 5, 4, 3, 0 };
        return GenerarClip("MusicaBossGenerada", notas, patron, 0.16f, 0.30f, false);
    }

    private static AudioClip GenerarClip(string nombre, float[] escalaHz, int[] patronIndices, float tiempoPorNota, float volumen, bool ondaSuave)
    {
        int muestrasPorNota = Mathf.RoundToInt(SAMPLE_RATE * tiempoPorNota);
        int totalMuestras = muestrasPorNota * patronIndices.Length;
        float[] datos = new float[totalMuestras];

        for (int n = 0; n < patronIndices.Length; n++)
        {
            float freq = escalaHz[patronIndices[n] % escalaHz.Length];
            int offset = n * muestrasPorNota;

            for (int i = 0; i < muestrasPorNota; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float sample = Mathf.Sin(2f * Mathf.PI * freq * t);
                if (ondaSuave)
                {
                    sample += 0.35f * Mathf.Sin(2f * Mathf.PI * freq * 2f * t);
                }
                else
                {
                    sample = Mathf.Sign(sample) * 0.6f + sample * 0.4f;
                }

                float envelope = EnvolventeNota((float)i / muestrasPorNota);
                datos[offset + i] = sample * envelope * volumen;
            }
        }

        AudioClip clip = AudioClip.Create(nombre, totalMuestras, 1, SAMPLE_RATE, false);
        clip.SetData(datos, 0);
        return clip;
    }

    private static float EnvolventeNota(float t)
    {
        const float ataque = 0.08f;
        const float liberacion = 0.25f;

        if (t < ataque) return t / ataque;
        if (t > 1f - liberacion) return Mathf.Clamp01((1f - t) / liberacion);
        return 1f;
    }
}
