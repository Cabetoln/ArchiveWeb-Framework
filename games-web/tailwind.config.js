/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        // Skin "arcade": fundo azul-noite profundo, acentos neon.
        bg: '#08080f',
        surface: '#101020',
        s2: '#181830',
        border: '#242444',
        ink: '#e8e8f6',
        muted: '#7a7aa0',
        neon: '#22d3ee',       // ciano — acento primário
        magenta: '#e94db8',    // magenta — acento secundário
        lime: '#9ef01a',       // verde-limão — economia/desconto
      },
      fontFamily: {
        display: ['Orbitron', 'sans-serif'],
        body: ['Rajdhani', 'sans-serif'],
      },
      boxShadow: {
        neon: '0 0 0 1px rgba(34,211,238,0.4), 0 0 22px -6px rgba(34,211,238,0.55)',
        magenta: '0 0 0 1px rgba(233,77,184,0.4), 0 0 22px -6px rgba(233,77,184,0.55)',
      },
    },
  },
  plugins: [],
}
