import { useForm } from "@tanstack/react-form"
import { useMutation, useQueryClient } from "@tanstack/react-query"
import { Link, useNavigate } from "@tanstack/react-router"
import { Plus } from "lucide-react"
import { toast } from "sonner"
import {
	createRoomMutation,
	listRoomsOptions,
} from "@/client/@tanstack/react-query.gen"
import type { RoomDto } from "@/client/types.gen"
import { zCreateRoomRequest } from "@/client/zod.gen"
import { Button } from "@/components/ui/button"
import {
	Dialog,
	DialogContent,
	DialogFooter,
	DialogHeader,
	DialogTitle,
	DialogTrigger,
} from "@/components/ui/dialog"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { ScrollArea } from "@/components/ui/scroll-area"
import { getApiErrorMessage } from "@/lib/api"

interface ChatSidebarProps {
	rooms: RoomDto[]
	activeRoomId?: string
}

export function ChatSidebar({ rooms, activeRoomId }: ChatSidebarProps) {
	const navigate = useNavigate()
	const queryClient = useQueryClient()
	const createRoom = useMutation({ ...createRoomMutation() })

	const form = useForm({
		defaultValues: { name: "" },
		validators: { onChange: zCreateRoomRequest },
		onSubmit: async ({ value }) => {
			try {
				const res = await createRoom.mutateAsync({ body: value })
				await queryClient.invalidateQueries({
					queryKey: listRoomsOptions().queryKey,
				})
				form.reset()
				const roomId = Array.isArray(res)
					? res[0]?.roomId
					: (res as { roomId?: string }).roomId
				if (roomId) navigate({ to: "/chat/$roomId", params: { roomId } })
			} catch (error) {
				toast.error(getApiErrorMessage(error as never))
			}
		},
	})

	return (
		<aside className="flex w-64 shrink-0 flex-col border-r bg-card">
			<div className="flex items-center justify-between border-b p-3">
				<h2 className="text-sm font-semibold">Rooms</h2>
				<Dialog>
					<DialogTrigger asChild>
						<Button size="icon-sm" variant="ghost">
							<Plus className="size-4" />
						</Button>
					</DialogTrigger>
					<DialogContent>
						<DialogHeader>
							<DialogTitle>New room</DialogTitle>
						</DialogHeader>
						<form
							onSubmit={(e) => {
								e.preventDefault()
								e.stopPropagation()
								form.handleSubmit()
							}}
							className="flex flex-col gap-4"
						>
							<form.Field name="name">
								{(field) => (
									<div className="flex flex-col gap-2">
										<Label htmlFor={field.name}>Name</Label>
										<Input
											id={field.name}
											name={field.name}
											value={field.state.value}
											onChange={(e) => field.handleChange(e.target.value)}
											onBlur={field.handleBlur}
										/>
										{field.state.meta.errors.length > 0 && (
											<p className="text-sm text-destructive">
												{field.state.meta.errors[0]?.message}
											</p>
										)}
									</div>
								)}
							</form.Field>
							<DialogFooter>
								<Button type="submit" disabled={createRoom.isPending}>
									{createRoom.isPending ? "Creating..." : "Create"}
								</Button>
							</DialogFooter>
						</form>
					</DialogContent>
				</Dialog>
			</div>
			<ScrollArea className="flex-1">
				<nav className="flex flex-col gap-1 p-2">
					{rooms.length === 0 && (
						<p className="p-2 text-sm text-muted-foreground">No rooms yet.</p>
					)}
					{rooms.map((room) => (
						<Link
							key={room.id}
							to="/chat/$roomId"
							params={{ roomId: room.id }}
							className={
								"rounded-md px-3 py-2 text-sm transition-colors hover:bg-accent " +
								(room.id === activeRoomId ? "bg-accent font-medium" : "")
							}
						>
							{room.name}
						</Link>
					))}
				</nav>
			</ScrollArea>
		</aside>
	)
}
