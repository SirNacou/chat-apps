import {
	createRoomMutation,
	listRoomsOptions,
	listUsersOptions,
} from "@/client/@tanstack/react-query.gen"
import type { RoomDto, UserResponseDto } from "@/client/types.gen"
import { zCreateRoomRequest } from "@/client/zod.gen"
import { Button } from "@/components/ui/button"
import {
	Collapsible,
	CollapsibleContent,
	CollapsibleTrigger,
} from "@/components/ui/collapsible"
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
import {
	Sidebar,
	SidebarContent,
	SidebarFooter,
	SidebarGroup,
	SidebarGroupContent,
	SidebarGroupLabel,
	SidebarHeader,
	SidebarMenu,
	SidebarMenuButton,
	SidebarMenuItem,
	SidebarRail,
} from "@/components/ui/sidebar"
import { UserAvatar } from "@/components/UserAvatar"
import { UserMenu } from "@/components/UserMenu"
import { getApiErrorMessage } from "@/lib/api"
import { useForm } from "@tanstack/react-form"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { Link, useNavigate } from "@tanstack/react-router"
import { ChevronDown, MessageCircle, Plus } from "lucide-react"
import { useState } from "react"
import { toast } from "sonner"
import type z from "zod"

interface ChatSidebarProps {
	rooms: RoomDto[]
	activeRoomId?: string
	email?: string
}

const baseUrl = "http://localhost:8080"

type defaultValues = z.infer<typeof zCreateRoomRequest>

export function ChatSidebar({ rooms, activeRoomId, email }: ChatSidebarProps) {
	const navigate = useNavigate()
	const queryClient = useQueryClient()
	const createRoom = useMutation({ ...createRoomMutation() })
	const [open, setOpen] = useState(false)
	const [peopleOpen, setPeopleOpen] = useState(true)
	const groupRooms = rooms.filter((r) => r.type !== "DirectMessage")

	const usersQuery = useQuery({ ...listUsersOptions(), retry: false })
	const users = (usersQuery.data?.data ?? []).filter((u) => u.username !== email)

	const createDM = useMutation({
		mutationFn: async (recipientId: string) => {
			const response = await fetch(
				`${baseUrl}/rooms/dm`,
				{
					method: "POST",
					credentials: "include",
					headers: { "Content-Type": "application/json" },
					body: JSON.stringify({ recipientId }),
				},
			)
			if (!response.ok) {
				const err = await response.json().catch(() => null)
				throw new Error(err?.detail ?? "Failed to create DM")
			}
			return (await response.json()) as { roomId: string }
		},
		onSuccess: async (data) => {
			await queryClient.invalidateQueries({
				queryKey: listRoomsOptions().queryKey,
			})
			if (data.roomId) {
				navigate({ to: "/chat/$roomId", params: { roomId: data.roomId } })
			}
		},
		onError: (error) => {
			toast.error(getApiErrorMessage(error as never))
		},
	})

	const form = useForm({
		defaultValues: { name: "" } as defaultValues,
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
		<Sidebar collapsible="icon">
			<SidebarHeader>
				<SidebarMenu>
					<SidebarMenuItem>
						<div className="flex items-center justify-between px-2">
							<span className="text-sm font-semibold group-data-[collapsible=icon]:hidden">
								Rooms
							</span>
							<Dialog open={open} onOpenChange={setOpen}>
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
											setOpen(false)
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
					</SidebarMenuItem>
				</SidebarMenu>
			</SidebarHeader>
			<SidebarContent>
				<SidebarGroup>
					<SidebarGroupContent>
						<SidebarMenu>
							{groupRooms.length === 0 && (
								<SidebarMenuItem>
									<span className="px-2 text-sm text-muted-foreground">
										No rooms yet.
									</span>
								</SidebarMenuItem>
							)}
							{groupRooms.map((room) => (
								<SidebarMenuItem key={room.id}>
									<SidebarMenuButton asChild isActive={room.id === activeRoomId}>
										<Link to="/chat/$roomId" params={{ roomId: room.id }}>
											<span>{room.name}</span>
										</Link>
									</SidebarMenuButton>
								</SidebarMenuItem>
							))}
						</SidebarMenu>
					</SidebarGroupContent>
				</SidebarGroup>
				<Collapsible open={peopleOpen} onOpenChange={setPeopleOpen}>
					<SidebarGroup>
						<CollapsibleTrigger asChild>
							<SidebarGroupLabel className="cursor-pointer">
								<ChevronDown
									className={`size-4 transition-transform ${peopleOpen ? "" : "-rotate-90"}`}
								/>
								<span>People</span>
							</SidebarGroupLabel>
						</CollapsibleTrigger>
						<CollapsibleContent>
							<SidebarGroupContent>
								<SidebarMenu>
									{users.length === 0 && (
										<SidebarMenuItem>
											<span className="px-2 text-sm text-muted-foreground">
												No users found.
											</span>
										</SidebarMenuItem>
									)}
									{users.map((user: UserResponseDto) => (
										<SidebarMenuItem key={user.id}>
											<SidebarMenuButton
												className="flex items-center gap-3"
												disabled={createDM.isPending}
												onClick={() => {
													if (user.id) createDM.mutate(user.id)
												}}
											>
												<UserAvatar
													name={user.username ?? user.id ?? ""}
													online={user.isOnline}
													className="size-6 shrink-0"
												/>
												<span className="flex-1 truncate">
													{user.username ?? "Unknown"}
												</span>
												<MessageCircle className="size-3.5 text-muted-foreground shrink-0" />
											</SidebarMenuButton>
										</SidebarMenuItem>
									))}
								</SidebarMenu>
							</SidebarGroupContent>
						</CollapsibleContent>
					</SidebarGroup>
				</Collapsible>
			</SidebarContent>
			<SidebarFooter>
				<SidebarMenu>
					<SidebarMenuItem>
						<UserMenu email={email!} />
					</SidebarMenuItem>
				</SidebarMenu>
			</SidebarFooter>
			<SidebarRail />
		</Sidebar>
	)
}
