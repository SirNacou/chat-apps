import {
	type InfiniteData,
	useInfiniteQuery,
	useMutation,
	useQuery,
	useQueryClient,
} from "@tanstack/react-query";
import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { Loader2 } from "lucide-react";
import { useEffect, useMemo, useRef, useState } from "react";
import { toast } from "sonner";
import {
	getMessagesInfiniteOptions,
	listRoomsOptions,
	listUsersOptions,
	sendMessagesMutation,
} from "@/client/@tanstack/react-query.gen";
import type {
	GetMessagesResponse,
	GetMessagesResponseMessageDto,
} from "@/client/types.gen";
import { AddMembersDialog } from "@/components/AddMembersDialog";
import { MessageBubble } from "@/components/MessageBubble";
import { MessageInput } from "@/components/MessageInput";
import { RoomHeader } from "@/components/RoomHeader";
import { ScrollArea } from "@/components/ui/scroll-area";
import { useAuth } from "@/hooks/useAuth";
import { type PresenceUpdate, useChatHub } from "@/hooks/useChatHub";
import { getApiErrorMessage } from "@/lib/api";

export const Route = createFileRoute("/_protected/chat/$roomId")({
	component: RoomView,
});

function RoomView() {
	const { roomId } = Route.useParams();
	const navigate = useNavigate();
	const auth = useAuth();

	useEffect(() => {
		if (auth.isError) navigate({ to: "/login" });
	}, [auth.isError, navigate]);

	if (auth.isPending || auth.isError || !auth.data) {
		return <div className="min-h-screen" />;
	}

	return <RoomViewInner auth={auth} roomId={roomId} />;
}

type MessagesInfinite = InfiniteData<GetMessagesResponse, unknown>;

function RoomViewInner({
	auth,
	roomId,
}: {
	auth: ReturnType<typeof useAuth>;
	roomId: string;
}) {
	const queryClient = useQueryClient();
	const [addMembersOpen, setAddMembersOpen] = useState(false);

	const { data: roomsData } = useQuery({ ...listRoomsOptions(), retry: false });
	const rooms = roomsData?.rooms ?? [];
	const activeRoom = rooms.find((r) => r.id === roomId);

	const usersQuery = useQuery({ ...listUsersOptions(), retry: false });
	const currentUserId = useMemo(() => {
		const me = usersQuery.data?.data?.find(
			(u) => u.username === auth.data?.email,
		);
		return me?.id;
	}, [usersQuery.data, auth.data?.email]);

	const messagesQueryKey = getMessagesInfiniteOptions({
		path: { roomId },
		query: { limit: 50 },
	}).queryKey;

	const query = useInfiniteQuery({
		...getMessagesInfiniteOptions({ path: { roomId }, query: { limit: 50 } }),
		retry: false,
		initialPageParam: "",
		getNextPageParam: (lastPage) => lastPage.nextCursor ?? undefined,
	});

	const sendMutation = useMutation({ ...sendMessagesMutation() });

	const messagesBySender = useMemo(() => {
		const map = new Map<string, string>();
		for (const u of usersQuery.data?.data ?? []) {
			if (u.id) map.set(u.id, u.username ?? u.id);
		}
		return map;
	}, [usersQuery.data]);

	const appendMessage = (message: GetMessagesResponseMessageDto) => {
		queryClient.setQueryData<MessagesInfinite>(messagesQueryKey, (old) => {
			if (!old) return old;
			const pages = old.pages.map((p, i) =>
				i === old.pages.length - 1
					? { ...p, messages: [...(p.messages ?? []), message] }
					: p,
			);
			return { ...old, pages };
		});
	};

	const { connected } = useChatHub({
		roomId,
		onMessage: (message) => {
			if (message.senderId !== currentUserId) {
				appendMessage(message);
			}
		},
		onPresence: (_: PresenceUpdate) => {},
	});

	const handleSend = async (content: string) => {
		const optimistic: GetMessagesResponseMessageDto = {
			id: `optimistic-${Date.now()}`,
			content,
			senderId: currentUserId ?? "",
			timestamp: new Date().toISOString(),
		};
		appendMessage(optimistic);
		try {
			await sendMutation.mutateAsync({
				path: { roomId },
				body: { content },
			});
		} catch (error) {
			toast.error(getApiErrorMessage(error as never));
		}
	};

	const allMessages = query.data?.pages.flatMap((p) => p.messages ?? []) ?? [];
	const bottomRef = useRef<HTMLDivElement>(null);

	// biome-ignore lint/correctness/useExhaustiveDependencies: scroll to bottom when the message list changes
	useEffect(() => {
		bottomRef.current?.scrollIntoView({ behavior: "auto" });
	}, [allMessages]);

	if (query.isLoading) {
		return (
			<div className="flex flex-1 items-center justify-center">
				<Loader2 className="size-6 animate-spin text-muted-foreground" />
			</div>
		);
	}

	return (
		<>
			<RoomHeader
				room={activeRoom}
				connected={connected}
				email={auth.data?.email}
				users={usersQuery.data?.data}
				onAddMembers={() => setAddMembersOpen(true)}
			/>
			<ScrollArea className="flex-1 p-4">
				<div className="flex flex-col gap-3">
					{query.hasNextPage && (
						<div className="flex justify-center">
							<button
								type="button"
								className="text-xs text-muted-foreground hover:text-foreground"
								onClick={() => query.fetchNextPage()}
								disabled={query.isFetchingNextPage}
							>
								{query.isFetchingNextPage
									? "Loading older..."
									: "Load older messages"}
							</button>
						</div>
					)}
					{allMessages.map((message) => (
						<MessageBubble
							key={message.id}
							message={message}
							isOwn={message.senderId === currentUserId}
							senderName={
								(message.senderId && messagesBySender.get(message.senderId)) ||
								"Unknown"
							}
							showSender
						/>
					))}
					<div ref={bottomRef} />
				</div>
			</ScrollArea>
			<MessageInput onSend={handleSend} disabled={!connected} />
			<AddMembersDialog
				roomId={roomId}
				open={addMembersOpen}
				onOpenChange={setAddMembersOpen}
				currentUserEmail={auth.data?.email}
			/>
		</>
	);
}
