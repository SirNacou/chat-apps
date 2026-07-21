import tailwindcss from "@tailwindcss/vite"
import { devtools } from "@tanstack/devtools-vite"

import { tanstackRouter } from "@tanstack/router-plugin/vite"
import viteReact, { reactCompilerPreset } from "@vitejs/plugin-react"
import { defineConfig } from "vite"
import babel from '@rolldown/plugin-babel'

const config = defineConfig({
	resolve: { tsconfigPaths: true },
	server: {
		watch: {
			usePolling: true
		}
	},
	plugins: [
		devtools(),
		tailwindcss(),
		tanstackRouter({ target: "react", autoCodeSplitting: true }),
		viteReact(),
		babel({
			presets: [reactCompilerPreset]
		})
	],
})

export default config
