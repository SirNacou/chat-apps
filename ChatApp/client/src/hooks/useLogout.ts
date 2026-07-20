import { useMutation } from "@tanstack/react-query";
import { useNavigate } from "@tanstack/react-router";
import { toast } from "sonner";
import { postLogoutMutation } from "@/client/@tanstack/react-query.gen";

export function useLogout() {
	const navigate = useNavigate();
	const mutation = useMutation({ ...postLogoutMutation() });

	return {
		isPending: mutation.isPending,
		logout: async () => {
			try {
				await mutation.mutateAsync({ body: {} as never });
			} catch {
				// ignore server errors, still clear client session
			} finally {
				toast.success("Signed out");
				navigate({ to: "/login" });
			}
		},
	};
}
