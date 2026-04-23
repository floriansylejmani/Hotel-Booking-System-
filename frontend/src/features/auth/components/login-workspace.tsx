"use client";

import Link from "next/link";
import { zodResolver } from "@hookform/resolvers/zod";
import { ArrowRight, LockKeyhole, UserCircle2 } from "lucide-react";
import { useRouter, useSearchParams } from "next/navigation";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { login as loginRequest } from "@/features/auth/api";
import { useAuth } from "@/hooks/use-auth";
import { useToast } from "@/hooks/use-toast";
import { getApiErrorMessage } from "@/services/api-client";

const loginSchema = z.object({
  email: z.email("Enter a valid email."),
  password: z.string().min(8, "Password must contain at least 8 characters."),
});

type LoginValues = z.infer<typeof loginSchema>;

const demoCredentials = [
  {
    label: "Admin",
    email: "admin@hotel.com",
    password: "Admin123!",
  },
  {
    label: "Guest",
    email: "james.carter@example.com",
    password: "Guest123!",
  },
];

export function LoginWorkspace() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const redirectTarget = searchParams.get("redirect") || "/dashboard";
  const { login } = useAuth();
  const { addToast } = useToast();
  const [submissionError, setSubmissionError] = useState<string | null>(null);

  const form = useForm<LoginValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: "admin@hotel.com",
      password: "Admin123!",
    },
  });

  const onSubmit = form.handleSubmit(async (values) => {
    setSubmissionError(null);

    try {
      const session = await loginRequest(values);
      login(session);
      addToast({
        title: "Signed in",
        description: `Welcome back, ${session.user.name}.`,
        tone: "success",
      });
      router.replace(redirectTarget);
    } catch (error) {
      setSubmissionError(getApiErrorMessage(error, "Unable to sign in right now."));
    }
  });

  return (
    <Card className="overflow-hidden bg-[linear-gradient(180deg,rgba(17,24,39,0.96),rgba(7,11,20,0.96))]">
      <CardHeader className="space-y-4">
        <div className="flex size-14 items-center justify-center rounded-[1.4rem] border border-white/8 bg-white/[0.05] text-accent-cyan">
          <LockKeyhole className="size-6" />
        </div>
        <div>
          <CardTitle>Sign in</CardTitle>
          <CardDescription>
            Use a seeded backend account or your registered guest account to enter
            the live hotel operations workspace.
          </CardDescription>
        </div>
      </CardHeader>
      <CardContent className="space-y-6">
        <div className="grid gap-3 sm:grid-cols-2">
          {demoCredentials.map((account) => (
            <button
              key={account.label}
              type="button"
              onClick={() => {
                form.reset({
                  email: account.email,
                  password: account.password,
                });
              }}
              className="rounded-[1.2rem] border border-surface-border bg-white/[0.03] px-4 py-3 text-left transition hover:border-surface-border-strong hover:bg-white/[0.06]"
            >
              <p className="text-sm font-semibold text-white">{account.label}</p>
              <p className="mt-1 text-xs text-muted-foreground">{account.email}</p>
            </button>
          ))}
        </div>

        <form onSubmit={onSubmit} className="space-y-4">
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

          {submissionError ? (
            <div className="rounded-2xl border border-accent-red/25 bg-accent-red/10 px-4 py-3 text-sm text-accent-red">
              {submissionError}
            </div>
          ) : null}

          <Button
            type="submit"
            className="w-full"
            size="lg"
            disabled={form.formState.isSubmitting}
          >
            {form.formState.isSubmitting ? "Signing in..." : "Sign in"}
            <ArrowRight className="size-4" />
          </Button>
        </form>

        <div className="rounded-[1.3rem] border border-surface-border bg-white/[0.03] p-4">
          <div className="flex items-center gap-3">
            <UserCircle2 className="size-5 text-accent-indigo" />
            <p className="text-sm text-slate-200">
              Need guest access for the real backend?
            </p>
          </div>
          <Link
            href="/register"
            className="mt-3 inline-flex text-sm font-semibold text-accent-cyan transition hover:text-white"
          >
            Create a guest account
          </Link>
        </div>
      </CardContent>
    </Card>
  );
}
