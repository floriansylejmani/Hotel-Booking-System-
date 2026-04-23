"use client";

import Link from "next/link";
import { zodResolver } from "@hookform/resolvers/zod";
import { ArrowRight, UserPlus } from "lucide-react";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Select } from "@/components/ui/select";
import { registerAccount } from "@/features/auth/api";
import { useAuth } from "@/hooks/use-auth";
import { useToast } from "@/hooks/use-toast";
import { getApiErrorMessage } from "@/services/api-client";

const registerSchema = z
  .object({
    name: z.string().min(3, "Name must contain at least 3 characters."),
    email: z.email("Enter a valid email."),
    password: z.string().min(8, "Password must contain at least 8 characters."),
    confirmPassword: z.string().min(8, "Confirm your password."),
  })
  .refine((values) => values.password === values.confirmPassword, {
    message: "Passwords do not match.",
    path: ["confirmPassword"],
  });

type RegisterValues = z.infer<typeof registerSchema>;

export function RegisterWorkspace() {
  const router = useRouter();
  const { login } = useAuth();
  const { addToast } = useToast();
  const [submissionError, setSubmissionError] = useState<string | null>(null);

  const form = useForm<RegisterValues>({
    resolver: zodResolver(registerSchema),
    defaultValues: {
      name: "James Carter",
      email: "new.guest@example.com",
      password: "Welcome123!",
      confirmPassword: "Welcome123!",
    },
  });

  const onSubmit = form.handleSubmit(async (values) => {
    setSubmissionError(null);

    try {
      const session = await registerAccount({
        fullName: values.name,
        email: values.email,
        password: values.password,
      });

      login(session);
      addToast({
        title: "Account created",
        description: `${session.user.name} can now access the live dashboard.`,
        tone: "success",
      });
      router.replace("/dashboard");
    } catch (error) {
      setSubmissionError(getApiErrorMessage(error, "Unable to create account."));
    }
  });

  return (
    <Card className="overflow-hidden bg-[linear-gradient(180deg,rgba(17,24,39,0.96),rgba(7,11,20,0.96))]">
      <CardHeader className="space-y-4">
        <div className="flex size-14 items-center justify-center rounded-[1.4rem] border border-white/8 bg-white/[0.05] text-accent-green">
          <UserPlus className="size-6" />
        </div>
        <div>
          <CardTitle>Create an account</CardTitle>
          <CardDescription>
            Registration is now connected to the backend auth service. Public
            sign-up provisions a guest account and signs it in immediately.
          </CardDescription>
        </div>
      </CardHeader>
      <CardContent>
        <form onSubmit={onSubmit} className="space-y-4">
          <div className="grid gap-4 md:grid-cols-2">
            <div className="space-y-2">
              <label className="text-xs uppercase tracking-[0.2em] text-muted-foreground">
                Full name
              </label>
              <Input {...form.register("name")} />
              {form.formState.errors.name ? (
                <p className="text-sm text-accent-red">
                  {form.formState.errors.name.message}
                </p>
              ) : null}
            </div>
            <div className="space-y-2">
              <label className="text-xs uppercase tracking-[0.2em] text-muted-foreground">
                Email
              </label>
              <Input {...form.register("email")} />
              {form.formState.errors.email ? (
                <p className="text-sm text-accent-red">
                  {form.formState.errors.email.message}
                </p>
              ) : null}
            </div>
          </div>

          <div className="space-y-2">
            <label className="text-xs uppercase tracking-[0.2em] text-muted-foreground">
              Role
            </label>
            <Select value="guest" disabled>
              <option value="guest">Guest</option>
            </Select>
            <p className="text-xs text-muted-foreground">
              Staff accounts continue to be seeded or managed server-side.
            </p>
          </div>

          <div className="grid gap-4 md:grid-cols-2">
            <div className="space-y-2">
              <label className="text-xs uppercase tracking-[0.2em] text-muted-foreground">
                Password
              </label>
              <Input type="password" {...form.register("password")} />
              {form.formState.errors.password ? (
                <p className="text-sm text-accent-red">
                  {form.formState.errors.password.message}
                </p>
              ) : null}
            </div>
            <div className="space-y-2">
              <label className="text-xs uppercase tracking-[0.2em] text-muted-foreground">
                Confirm password
              </label>
              <Input type="password" {...form.register("confirmPassword")} />
              {form.formState.errors.confirmPassword ? (
                <p className="text-sm text-accent-red">
                  {form.formState.errors.confirmPassword.message}
                </p>
              ) : null}
            </div>
          </div>

          {submissionError ? (
            <div className="rounded-2xl border border-accent-red/25 bg-accent-red/10 px-4 py-3 text-sm text-accent-red">
              {submissionError}
            </div>
          ) : null}

          <Button
            type="submit"
            size="lg"
            className="w-full"
            disabled={form.formState.isSubmitting}
          >
            {form.formState.isSubmitting ? "Creating account..." : "Create account"}
            <ArrowRight className="size-4" />
          </Button>
        </form>

        <p className="mt-6 text-sm text-muted-foreground">
          Already have access?{" "}
          <Link href="/login" className="font-semibold text-accent-cyan">
            Sign in
          </Link>
        </p>
      </CardContent>
    </Card>
  );
}
