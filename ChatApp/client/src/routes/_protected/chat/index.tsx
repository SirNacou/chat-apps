import { createFileRoute } from "@tanstack/react-router"

export const Route = createFileRoute("/_protected/chat/")({
	component: ChatIndex,
})

function ChatIndex() {
	return (
		<div className="flex flex-1 items-center justify-center text-muted-foreground">
			Select a room to start chatting
		</div>
	)
}
