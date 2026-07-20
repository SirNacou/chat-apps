import { useForm } from "@tanstack/react-form";
import { useMutation } from "@tanstack/react-query";
import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { toast } from "sonner";
import { postRegisterMutation } from "@/client/@tanstack/react-query.gen";
import { zRegisterRequest } from "@/client/zod.gen";

import { Button } from "@/components/ui/button";
import {
	Card,
	CardContent,
	CardDescription,
	CardHeader,
	CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { getApiErrorMessage } from "@/lib/api";

export const Route = createFileRoute("/register")({
	component: RegisterPage,
});

function RegisterPage() {
	const navigate = useNavigate();
	const mutation = useMutation({ ...postRegisterMutation() });

	const form = useForm({
		defaultValues: {
			email: "",
			password: "",
		},
		validators: {
			onChange: zRegisterRequest,
		},
		onSubmit: async ({ value }) => {
			try {
				await mutation.mutateAsync({ body: value });
				toast.success("Account created. You can sign in now.");
				navigate({ to: "/login" });
			} catch (error) {
				toast.error(getApiErrorMessage(error as never));
			}
		},
	});

	return (
		<div className="flex min-h-screen items-center justify-center p-4">
			<Card className="w-full max-w-sm">
				<CardHeader>
					<CardTitle className="text-2xl">Create account</CardTitle>
					<CardDescription>Sign up to start chatting.</CardDescription>
				</CardHeader>
				<CardContent>
					<form
						onSubmit={(e) => {
							e.preventDefault();
							e.stopPropagation();
							form.handleSubmit();
						}}
						className="flex flex-col gap-4"
					>
						<form.Field name="email">
							{(field) => (
								<div className="flex flex-col gap-2">
									<Label htmlFor={field.name}>Email</Label>
									<Input
										id={field.name}
										name={field.name}
										type="email"
										autoComplete="email"
										value={field.state.value}
										onBlur={field.handleBlur}
										onChange={(e) => field.handleChange(e.target.value)}
									/>
									{field.state.meta.errors.length > 0 && (
										<p className="text-sm text-destructive">
											{field.state.meta.errors[0]?.message}
										</p>
									)}
								</div>
							)}
						</form.Field>

						<form.Field name="password">
							{(field) => (
								<div className="flex flex-col gap-2">
									<Label htmlFor={field.name}>Password</Label>
									<Input
										id={field.name}
										name={field.name}
										type="password"
										autoComplete="new-password"
										value={field.state.value}
										onBlur={field.handleBlur}
										onChange={(e) => field.handleChange(e.target.value)}
									/>
									{field.state.meta.errors.length > 0 && (
										<p className="text-sm text-destructive">
											{field.state.meta.errors[0]?.message}
										</p>
									)}
								</div>
							)}
						</form.Field>

						<Button type="submit" disabled={mutation.isPending}>
							{mutation.isPending ? "Creating..." : "Create account"}
						</Button>

						<p className="text-center text-sm text-muted-foreground">
							Already have an account?{" "}
							<a
								className="underline underline-offset-4 hover:text-foreground"
								href="/login"
							>
								Sign in
							</a>
						</p>
					</form>
				</CardContent>
			</Card>
		</div>
	);
}
