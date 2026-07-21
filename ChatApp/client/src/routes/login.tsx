import { useForm } from "@tanstack/react-form"
import { useMutation } from "@tanstack/react-query"
import { createFileRoute, Link, useNavigate } from "@tanstack/react-router"
import { toast } from "sonner"
import { postLoginMutation } from "@/client/@tanstack/react-query.gen"
import { zLoginRequest } from "@/client/zod.gen"

import { Button } from "@/components/ui/button"
import {
	Card,
	CardContent,
	CardDescription,
	CardHeader,
	CardTitle,
} from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { getApiErrorMessage } from "@/lib/api"

export const Route = createFileRoute("/login")({
	component: LoginPage,
})

function LoginPage() {
	const navigate = useNavigate()
	const mutation = useMutation({ ...postLoginMutation() })

	const form = useForm({
		defaultValues: {
			email: "",
			password: "",
		},
		validators: {
			onChange: zLoginRequest,
		},
		onSubmit: async ({ value }) => {
			try {
				await mutation.mutateAsync({
					body: value,
					query: { useCookies: true },
				})
				toast.success("Signed in")
				navigate({ to: "/chat" })
			} catch (error) {
				toast.error(getApiErrorMessage(error as never))
			}
		},
	})

	return (
		<div className="flex min-h-screen items-center justify-center p-4">
			<Card className="w-full max-w-sm">
				<CardHeader>
					<CardTitle className="text-2xl">Sign in</CardTitle>
					<CardDescription>
						Enter your credentials to access your chats.
					</CardDescription>
				</CardHeader>
				<CardContent>
					<form
						onSubmit={(e) => {
							e.preventDefault()
							e.stopPropagation()
							form.handleSubmit()
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
										autoComplete="current-password"
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
							{mutation.isPending ? "Signing in..." : "Sign in"}
						</Button>

						<p className="text-center text-sm text-muted-foreground">
							No account?{" "}
							<Link
								className="underline underline-offset-4 hover:text-foreground"
								to="/register"
							>
								Create one
							</Link>
						</p>
					</form>
				</CardContent>
			</Card>
		</div>
	)
}
