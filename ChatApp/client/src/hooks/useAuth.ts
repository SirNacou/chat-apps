import { useQuery } from "@tanstack/react-query";
import { getManageInfoOptions } from "@/client/@tanstack/react-query.gen";

export interface AuthUser {
	email: string;
	isEmailConfirmed: boolean;
}

export function useAuth() {
	return useQuery({
		...getManageInfoOptions(),
		staleTime: 30_000,
		retry: false,
	});
}
