import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { cn } from "@/lib/utils";

interface UserAvatarProps {
	name: string;
	online?: boolean;
	className?: string;
}

function initials(name: string): string {
	const base = name.split("@")[0] || name || "?";
	return base.slice(0, 2).toUpperCase();
}

export function UserAvatar({ name, online, className }: UserAvatarProps) {
	return (
		<span className="relative inline-flex">
			<Avatar className={cn("size-8", className)}>
				<AvatarFallback className="text-xs">{initials(name)}</AvatarFallback>
			</Avatar>
			{online !== undefined && (
				<span
					className={cn(
						"absolute -bottom-0.5 -right-0.5 size-3 rounded-full border-2 border-background",
						online ? "bg-green-500" : "bg-muted-foreground/40",
					)}
				/>
			)}
		</span>
	);
}
