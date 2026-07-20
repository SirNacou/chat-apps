import type { GetMessagesResponseMessageDto } from "@/client/types.gen";
import { cn } from "@/lib/utils";
import { UserAvatar } from "./UserAvatar";

interface MessageBubbleProps {
	message: GetMessagesResponseMessageDto;
	isOwn: boolean;
	senderName: string;
	showSender?: boolean;
}

function formatTime(timestamp: string): string {
	const date = new Date(timestamp);
	if (Number.isNaN(date.getTime())) return "";
	return date.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
}

export function MessageBubble({
	message,
	isOwn,
	senderName,
	showSender,
}: MessageBubbleProps) {
	return (
		<div className={cn("flex gap-3", isOwn && "flex-row-reverse")}>
			{showSender && !isOwn && (
				<UserAvatar name={senderName} className="mt-1 size-7" />
			)}
			<div
				className={cn("flex max-w-[75%] flex-col gap-1", isOwn && "items-end")}
			>
				{showSender && !isOwn && (
					<span className="px-1 text-xs text-muted-foreground">
						{senderName}
					</span>
				)}
				<div
					className={cn(
						"rounded-2xl px-4 py-2 text-sm",
						isOwn
							? "bg-primary text-primary-foreground"
							: "bg-muted text-foreground",
					)}
				>
					{message.content}
				</div>
				<span className="px-1 text-[10px] text-muted-foreground">
					{formatTime(message.timestamp)}
				</span>
			</div>
		</div>
	);
}
