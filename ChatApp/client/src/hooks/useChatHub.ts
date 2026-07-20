import {
	type HubConnection,
	HubConnectionBuilder,
	LogLevel,
} from "@microsoft/signalr";
import { useEffect, useRef, useState } from "react";

import type { GetMessagesResponseMessageDto } from "@/client/types.gen";

const HUB_URL = "http://localhost:8080/chathub";

export interface PresenceUpdate {
	userId: string;
	online: boolean;
	timestamp?: string;
}

interface UseChatHubOptions {
	roomId?: string;
	onMessage: (message: GetMessagesResponseMessageDto) => void;
	onPresence: (update: PresenceUpdate) => void;
}

export function useChatHub({
	roomId,
	onMessage,
	onPresence,
}: UseChatHubOptions) {
	const connectionRef = useRef<HubConnection | null>(null);
	const [connected, setConnected] = useState(false);

	const callbacksRef = useRef({ onMessage, onPresence });
	callbacksRef.current = { onMessage, onPresence };

	const roomIdRef = useRef(roomId);
	roomIdRef.current = roomId;

	useEffect(() => {
		const connection = new HubConnectionBuilder()
			.withUrl(HUB_URL, { withCredentials: true })
			.withAutomaticReconnect()
			.configureLogging(LogLevel.Warning)
			.build();

		connection.on(
			"ReceiveMessage",
			(message: GetMessagesResponseMessageDto) => {
				callbacksRef.current.onMessage(message);
			},
		);
		connection.on("UserConnected", (userId: string) => {
			callbacksRef.current.onPresence({ userId, online: true });
		});
		connection.on("UserDisconnected", (userId: string, timestamp: string) => {
			callbacksRef.current.onPresence({ userId, online: false, timestamp });
		});

		connection.onreconnected(async () => {
			setConnected(true);
			const currentRoom = roomIdRef.current;
			if (currentRoom) await connection.invoke("JoinRoom", currentRoom);
		});
		connection.onclose(() => setConnected(false));

		connection
			.start()
			.then(() => {
				setConnected(true);
			})
			.catch(() => setConnected(false));

		connectionRef.current = connection;

		return () => {
			connection.stop().catch(() => {});
			connectionRef.current = null;
			setConnected(false);
		};
	}, []);

	useEffect(() => {
		const connection = connectionRef.current;
		if (!connection || !connected || !roomId) return;

		connection.invoke("JoinRoom", roomId).catch(() => {});

		return () => {
			connection.invoke("LeaveRoom", roomId).catch(() => {});
		};
	}, [roomId, connected]);

	return { connected };
}
