import { createFileRoute, useNavigate } from "@tanstack/react-router"
import { useEffect } from "react"

import { useAuth } from "@/hooks/useAuth"

export const Route = createFileRoute("/_protected/")({
	component: Home,
})

function Home() {
	const navigate = useNavigate()
	const auth = useAuth()

	useEffect(() => {
		if (auth.isPending) return
		if (auth.data) navigate({ to: "/chat" })
		else navigate({ to: "/login" })
	}, [auth.isPending, auth.data, navigate])

	return <div className="min-h-screen" />
}
