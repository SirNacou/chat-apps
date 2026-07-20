import type { RoomDto } from "@/client/types.gen";

import { UserMenu } from "./UserMenu";

interface RoomHeaderProps {
	room?: RoomDto;
	email: string;
	connected: boolean;
}

export function RoomHeader({ room, email, connected }: RoomHeaderProps) {
	return (
		<header className="flex items-center justify-between border-b p-3">
			<div className="flex items-center gap-3">
				<h1 className="text-base font-semibold">{room?.name ?? "Chat"}</h1>
				<span
					className={
						"size-2 rounded-full " +
						(connected ? "bg-green-500" : "bg-muted-foreground/40")
					}
					title={connected ? "Realtime connected" : "Realtime disconnected"}
				/>
			</div>
			<UserMenu email={email} />
		</header>
	);
}
