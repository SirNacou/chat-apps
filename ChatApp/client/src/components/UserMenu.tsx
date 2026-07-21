import { ChevronUp, LogOut } from "lucide-react";

import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import {
	DropdownMenu,
	DropdownMenuContent,
	DropdownMenuItem,
	DropdownMenuSeparator,
	DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { SidebarMenuButton } from "@/components/ui/sidebar";

import { useLogout } from "@/hooks/useLogout";

interface UserMenuProps {
	email: string;
}

function initials(email: string): string {
	const name = email.split("@")[0] ?? "?";
	return name.slice(0, 2).toUpperCase();
}

function stringToHue(str: string): number {
	let hash = 0;
	for (let i = 0; i < str.length; i++) {
		hash = str.charCodeAt(i) + ((hash << 5) - hash);
	}
	return hash % 360;
}

export function UserMenu({ email }: UserMenuProps) {
	const { logout, isPending } = useLogout();
	const hue = stringToHue(email);

	return (
		<DropdownMenu>
			<DropdownMenuTrigger asChild>
				<SidebarMenuButton
					size="lg"
					className="data-[state=open]:bg-sidebar-accent data-[state=open]:text-sidebar-accent-foreground"
				>
					<Avatar className="size-6 shrink-0 rounded-lg">
						<AvatarFallback
							className="rounded-lg text-xs text-white"
							style={{ backgroundColor: `oklch(60% 0.15 ${hue})` }}
						>
							{initials(email)}
						</AvatarFallback>
					</Avatar>
					<div className="grid flex-1 text-left text-sm leading-tight group-data-[collapsible=icon]:hidden">
						<span className="truncate font-medium">{email.split("@")[0]}</span>
						<span className="truncate text-xs text-muted-foreground">
							{email}
						</span>
					</div>
					<ChevronUp className="ml-auto size-4 group-data-[collapsible=icon]:hidden" />
				</SidebarMenuButton>
			</DropdownMenuTrigger>
			<DropdownMenuContent
				side="top"
				align="start"
				className="w-(--radix-popper-anchor-width) min-w-56 rounded-lg"
			>
				<div className="flex items-center gap-3 px-2 py-3">
					<Avatar className="size-10 rounded-lg">
						<AvatarFallback
							className="rounded-lg text-sm text-white"
							style={{ backgroundColor: `oklch(60% 0.15 ${hue})` }}
						>
							{initials(email)}
						</AvatarFallback>
					</Avatar>
					<div className="grid flex-1 text-left text-sm leading-tight">
						<span className="truncate font-medium">{email.split("@")[0]}</span>
						<span className="truncate text-xs text-muted-foreground">
							{email}
						</span>
					</div>
				</div>
				<DropdownMenuSeparator />
				<DropdownMenuItem
					className="cursor-pointer text-destructive focus:text-destructive"
					disabled={isPending}
					onClick={() => logout()}
				>
					<LogOut className="size-4" />
					Sign out
				</DropdownMenuItem>
			</DropdownMenuContent>
		</DropdownMenu>
	);
}
