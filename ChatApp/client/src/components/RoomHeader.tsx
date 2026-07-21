import { UserPlus } from "lucide-react";
import type { RoomDto, UserResponseDto } from "@/client/types.gen";
import { Button } from "@/components/ui/button";

interface RoomHeaderProps {
	room?: RoomDto;
	connected: boolean;
	email?: string;
	users?: UserResponseDto[];
	onAddMembers?: () => void;
}

export function RoomHeader({
	room,
	connected,
	email,
	users,
	onAddMembers,
}: RoomHeaderProps) {
	const displayName =
		room?.type === "DirectMessage" && users && email
			? users
					.filter((u) => u.username !== email)
					.map((u) => u.username || u.id)
					.join(", ")
			: (room?.name ?? "Chat");

	return (
		<header className="flex items-center justify-between border-b p-3">
			<div className="flex items-center gap-3">
				<h1 className="text-base font-semibold">{displayName}</h1>
				<span
					className={
						"size-2 rounded-full " +
						(connected ? "bg-green-500" : "bg-muted-foreground/40")
					}
					title={connected ? "Realtime connected" : "Realtime disconnected"}
				/>
			</div>
			{room?.type === "Group" && onAddMembers && (
				<Button variant="outline" size="sm" onClick={onAddMembers}>
					<UserPlus className="size-4" />
					Add Members
				</Button>
			)}
		</header>
	);
}
