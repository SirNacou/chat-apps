import { zSendMessagesRequest } from "@/client/zod.gen"
import { useForm } from "@tanstack/react-form"
import { Send } from "lucide-react"

import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { z } from "zod"

interface MessageInputProps {
	onSend: (content: string) => void
	disabled?: boolean
}

type request = z.infer<typeof zSendMessagesRequest>

export function MessageInput({ onSend, disabled }: MessageInputProps) {
	const form = useForm({
		defaultValues: {
			content: "",
		} as request,
		validators: {
			onChange: zSendMessagesRequest,
		},
		onSubmit: async ({ value }) => {
			if (!value.content?.trim()) return
			onSend(value.content?.trim())
			form.reset()
		},
	})

	return (
		<form
			onSubmit={(e) => {
				e.preventDefault()
				e.stopPropagation()
				form.handleSubmit()
			}}
			className="flex items-center gap-2 border-t p-3"
		>
			<form.Field name="content">
				{(field) => (
					<Input
						id={field.name}
						name={field.name}
						autoComplete="off"
						placeholder="Type a message..."
						value={field.state.value}
						disabled={disabled}
						onChange={(e) => field.handleChange(e.target.value)}
						onBlur={field.handleBlur}
						className="flex-1"
					/>
				)}
			</form.Field>
			<Button type="submit" size="icon" disabled={disabled}>
				<Send className="size-4" />
			</Button>
		</form>
	)
}
