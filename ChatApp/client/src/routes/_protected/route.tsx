import { listRoomsOptions } from "@/client/@tanstack/react-query.gen"
import { useQuery } from "@tanstack/react-query"
import { createFileRoute, Outlet, useParams, useNavigate } from "@tanstack/react-router"
import { useEffect } from "react"

import { ChatSidebar } from "@/components/ChatSidebar"
import { SidebarProvider } from "@/components/ui/sidebar"
import { useAuth } from "@/hooks/useAuth"

export const Route = createFileRoute("/_protected")({
	component: RouteComponent,
})

function RouteComponent() {
	const navigate = useNavigate()
	const auth = useAuth()

	useEffect(() => {
		if (auth.isError) navigate({ to: "/login" })
	}, [auth.isError, navigate])

	if (auth.isPending || auth.isError || !auth.data) {
		return <div className="min-h-screen" />
	}

	return <ProtectedLayout />
}

function ProtectedLayout() {
	const auth = useAuth()
	const { data } = useQuery({ ...listRoomsOptions(), retry: false })
	const rooms = data?.rooms ?? []
	const { roomId } = useParams({ strict: false })

	return (
		<SidebarProvider>
			<ChatSidebar rooms={rooms} activeRoomId={roomId} email={auth.data?.email} />
			<main className="flex flex-1 flex-col">
				<Outlet />
			</main>
		</SidebarProvider>
	)
}
