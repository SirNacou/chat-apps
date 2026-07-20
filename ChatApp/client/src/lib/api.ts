import type { DefaultError } from "@tanstack/react-query";

export interface ProblemDetailsError {
	detail?: string | null;
	errors?: Array<{ name?: string; reason?: string }>;
	title?: string | null;
}

export function getApiErrorMessage(error: DefaultError): string {
	const data = (error as unknown as { data?: ProblemDetailsError }).data;
	if (data) {
		if (data.errors && data.errors.length > 0) {
			return data.errors
				.map((e) => e.reason ?? e.name ?? "")
				.join("\n")
				.trim();
		}
		if (data.detail) return data.detail;
		if (data.title) return data.title;
	}
	if (typeof error.message === "string" && error.message) return error.message;
	return "Something went wrong. Please try again.";
}
