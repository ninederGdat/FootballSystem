/** @type {import('tailwindcss').Config} */
export default {
  content: ["./index.html", "./src/**/*.{ts,tsx}"],
  theme: {
    extend: {
      colors: {
        // Primary brand color and a tonal ramp built around it (#001489).
        brand: {
          50: "#eef0fb",
          100: "#d6dbf3",
          200: "#adb6e7",
          300: "#8391da",
          400: "#4a58bc",
          500: "#001489", // brand base
          600: "#00117a",
          700: "#000e63",
          800: "#000a4d",
          900: "#000736",
        },
        surface: {
          DEFAULT: "#ffffff",
          subtle: "#f5f6fa",
          border: "#e2e4ee",
        },
        ink: {
          DEFAULT: "#14161f",
          muted: "#5b5f72",
          faint: "#8a8ea3",
        },
        success: "#0f8a4b",
        warning: "#b7791f",
        danger: "#c02b3a",
      },
      fontFamily: {
        sans: [
          "Inter",
          "ui-sans-serif",
          "system-ui",
          "-apple-system",
          "sans-serif",
        ],
      },
    },
  },
  plugins: [],
};
