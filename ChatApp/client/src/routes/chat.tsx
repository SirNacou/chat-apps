import { useQuery } from "@tanstack/react-query";
import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { useEffect } from "react";
import { listRoomsOptions } from "@/client/@tanstack/react-query.gen";

import { ChatSidebar } from "@/components/ChatSidebar";
import { RoomHeader } from "@/components/RoomHeader";
import { useAuth } from "@/hooks/useAuth";

export const Route = createFileRoute("/chat")({
	component: ChatLayout,
});

function ChatLayout() {
	const navigate = useNavigate();
	const auth = useAuth();

	useEffect(() => {
		if (auth.isError) navigate({ to: "/login" });
	}, [auth.isError, navigate]);

	if (auth.isPending || auth.isError || !auth.data) {
		return <div className="min-h-screen" />;
	}

	return <ChatRooms email={auth.data.email} />;
}

function ChatRooms({ email }: { email: string }) {
	const navigate = useNavigate();
	const { data } = useQuery({ ...listRoomsOptions(), retry: false });

	const rooms = (data ?? []).flatMap((r) => r.rooms);

	return (
		<div className="flex h-screen">
			<ChatSidebar rooms={rooms} />
			<div className="flex flex-1 flex-col">
				<RoomHeader email={email} connected={false} />
				<div className="flex flex-1 items-center justify-center text-muted-foreground">
					<div className="text-center">
						<p className="text-lg font-medium">Select a room</p>
						<p className="text-sm">
							Choose a conversation from the sidebar to start chatting.
						</p>
						{rooms[0] && (
							<button
								type="button"
								className="mt-4 text-sm underline underline-offset-4"
								onClick={() =>
									navigate({
										to: "/chat/$roomId",
										params: { roomId: rooms[0].id },
									})
								}
							>
								Open {rooms[0].name}
							</button>
						)}
					</div>
				</div>
			</div>
		</div>
	);
}
