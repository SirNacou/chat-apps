import { LogOut, User as UserIcon } from "lucide-react";

import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import {
	DropdownMenu,
	DropdownMenuContent,
	DropdownMenuItem,
	DropdownMenuLabel,
	DropdownMenuSeparator,
	DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

import { useLogout } from "@/hooks/useLogout";

interface UserMenuProps {
	email: string;
}

function initials(email: string): string {
	const name = email.split("@")[0] ?? "?";
	return name.slice(0, 2).toUpperCase();
}

export function UserMenu({ email }: UserMenuProps) {
	const { logout, isPending } = useLogout();

	return (
		<DropdownMenu>
			<DropdownMenuTrigger asChild>
				<Button variant="ghost" className="flex items-center gap-2 px-2">
					<Avatar className="size-7">
						<AvatarFallback className="text-xs">
							{initials(email)}
						</AvatarFallback>
					</Avatar>
					<span className="max-w-[160px] truncate text-sm">{email}</span>
				</Button>
			</DropdownMenuTrigger>
			<DropdownMenuContent align="end" className="w-56">
				<DropdownMenuLabel className="flex items-center gap-2">
					<UserIcon className="size-4" />
					<span className="truncate">{email}</span>
				</DropdownMenuLabel>
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
