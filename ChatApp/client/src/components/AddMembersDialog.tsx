import { useMutation, useQuery } from "@tanstack/react-query";
import { Check, Loader2, UserPlus, X } from "lucide-react";
import { useEffect, useState } from "react";
import { toast } from "sonner";
import {
	addMembersMutation,
	listUsersOptions,
} from "@/client/@tanstack/react-query.gen";
import { UserAvatar } from "@/components/UserAvatar";
import { Button } from "@/components/ui/button";
import {
	Dialog,
	DialogContent,
	DialogFooter,
	DialogHeader,
	DialogTitle,
} from "@/components/ui/dialog";
import { getApiErrorMessage } from "@/lib/api";

interface AddMembersDialogProps {
	roomId: string;
	open: boolean;
	onOpenChange: (open: boolean) => void;
	currentUserEmail?: string;
}

export function AddMembersDialog({
	roomId,
	open,
	onOpenChange,
	currentUserEmail,
}: AddMembersDialogProps) {
	const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

	useEffect(() => {
		if (open) setSelectedIds(new Set());
	}, [open]);

	const usersQuery = useQuery({ ...listUsersOptions(), retry: false });
	const users = (usersQuery.data?.data ?? []).filter(
		(u): u is typeof u & { id: string } =>
			u.username !== currentUserEmail && !!u.id,
	);

	const addMembers = useMutation({ ...addMembersMutation() });

	const handleToggle = (userId: string) => {
		setSelectedIds((prev) => {
			const next = new Set(prev);
			if (next.has(userId)) {
				next.delete(userId);
			} else {
				next.add(userId);
			}
			return next;
		});
	};

	const handleSubmit = async () => {
		if (selectedIds.size === 0) return;
		try {
			await addMembers.mutateAsync({
				path: { roomId },
				body: { memberIds: Array.from(selectedIds) },
			});
			toast.success("Members added successfully");
			setSelectedIds(new Set());
			onOpenChange(false);
		} catch (error) {
			toast.error(getApiErrorMessage(error as never));
		}
	};

	return (
		<Dialog open={open} onOpenChange={onOpenChange}>
			<DialogContent>
				<DialogHeader>
					<DialogTitle>Add Members</DialogTitle>
				</DialogHeader>
				<div className="flex max-h-64 flex-col gap-1 overflow-y-auto">
					{usersQuery.isLoading ? (
						<div className="flex items-center justify-center py-8">
							<Loader2 className="size-5 animate-spin text-muted-foreground" />
						</div>
					) : users.length === 0 ? (
						<p className="py-4 text-center text-sm text-muted-foreground">
							No users to add.
						</p>
					) : (
						users.map((user) => {
							const isSelected = selectedIds.has(user.id);
							return (
								<button
									key={user.id}
									type="button"
									onClick={() => handleToggle(user.id)}
									className="flex items-center gap-3 rounded-md px-2 py-2 text-left transition-colors hover:bg-accent"
								>
									<div className="flex size-5 shrink-0 items-center justify-center rounded-sm border border-input">
										{isSelected && <Check className="size-3.5 text-primary" />}
									</div>
									<UserAvatar
										name={user.username ?? user.id ?? ""}
										online={user.isOnline}
										className="size-7 shrink-0"
									/>
									<span className="flex-1 truncate text-sm">
										{user.username ?? "Unknown"}
									</span>
								</button>
							);
						})
					)}
				</div>
				<DialogFooter>
					<Button
						type="button"
						variant="outline"
						onClick={() => onOpenChange(false)}
					>
						<X className="size-4" />
						Cancel
					</Button>
					<Button
						type="button"
						onClick={handleSubmit}
						disabled={selectedIds.size === 0 || addMembers.isPending}
					>
						{addMembers.isPending ? (
							<>
								<Loader2 className="size-4 animate-spin" />
								Adding...
							</>
						) : (
							<>
								<UserPlus className="size-4" />
								Add{selectedIds.size > 0 ? ` (${selectedIds.size})` : ""}
							</>
						)}
					</Button>
				</DialogFooter>
			</DialogContent>
		</Dialog>
	);
}
