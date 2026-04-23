"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Download, Printer } from "lucide-react";
import { useMemo, useState } from "react";
import { useForm, useWatch } from "react-hook-form";
import { z } from "zod";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { SectionHeading } from "@/components/ui/section-heading";
import { Select } from "@/components/ui/select";
import { getBookings } from "@/features/bookings/api";
import {
  downloadInvoicePdf,
  getDefaultInvoice,
  invoiceQueryKey,
  processPayment,
} from "@/features/payments/api";
import { useAuth } from "@/hooks/use-auth";
import { useToast } from "@/hooks/use-toast";
import { formatCompactDate, formatPreciseCurrency } from "@/lib/format";
import { hasPermission } from "@/lib/permissions";
import { getApiErrorMessage } from "@/services/api-client";

const paymentSchema = z
  .object({
    paymentMethod: z.enum(["credit-card", "cash", "check"]),
    cardNumber: z.string().optional(),
    expiry: z.string().optional(),
    cvv: z.string().optional(),
    checkNumber: z.string().optional(),
    cashReference: z.string().optional(),
  })
  .superRefine((values, context) => {
    if (values.paymentMethod === "credit-card") {
      if (!values.cardNumber || values.cardNumber.length < 12) {
        context.addIssue({
          code: "custom",
          path: ["cardNumber"],
          message: "Card number is required.",
        });
      }
      if (!values.expiry) {
        context.addIssue({
          code: "custom",
          path: ["expiry"],
          message: "Expiry is required.",
        });
      }
      if (!values.cvv || values.cvv.length < 3) {
        context.addIssue({
          code: "custom",
          path: ["cvv"],
          message: "CVV is required.",
        });
      }
    }

    if (values.paymentMethod === "check" && !values.checkNumber) {
      context.addIssue({
        code: "custom",
        path: ["checkNumber"],
        message: "Check number is required.",
      });
    }

    if (values.paymentMethod === "cash" && !values.cashReference) {
      context.addIssue({
        code: "custom",
        path: ["cashReference"],
        message: "Cash reference is required.",
      });
    }
  });

type PaymentFormValues = z.infer<typeof paymentSchema>;

export function PaymentsWorkspace() {
  const queryClient = useQueryClient();
  const { user } = useAuth();
  const { addToast } = useToast();
  const [selectedBookingId, setSelectedBookingId] = useState("");
  const canManagePayments = user ? hasPermission(user.role, "payments.manage") : false;

  const bookingsQuery = useQuery({
    queryKey: ["bookings", "for-payments"],
    queryFn: () => getBookings({ pageSize: 100 }),
  });

  const availableBookings = useMemo(
    () => bookingsQuery.data?.items.filter((booking) => booking.status !== "cancelled") ?? [],
    [bookingsQuery.data],
  );
  const activeBookingId = selectedBookingId || availableBookings[0]?.recordId || "";

  const invoiceQuery = useQuery({
    queryKey: [...invoiceQueryKey, activeBookingId],
    queryFn: () => getDefaultInvoice(activeBookingId),
    enabled: !!activeBookingId,
  });

  const paymentMutation = useMutation({
    mutationFn: ({
      bookingId,
      paymentMethod,
      amount,
    }: {
      bookingId: string;
      paymentMethod: PaymentFormValues["paymentMethod"];
      amount: number;
    }) => processPayment(bookingId, paymentMethod, amount),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: invoiceQueryKey }),
        queryClient.invalidateQueries({ queryKey: ["bookings"] }),
        queryClient.invalidateQueries({ queryKey: ["dashboard"] }),
        queryClient.invalidateQueries({ queryKey: ["notifications"] }),
      ]);
      addToast({
        title: "Payment processed",
        description: "The payment was posted successfully.",
        tone: "success",
      });
    },
    onError: (error) => {
      addToast({
        title: "Unable to process payment",
        description: getApiErrorMessage(error, "Payment processing failed."),
        tone: "warning",
      });
    },
  });

  const form = useForm<PaymentFormValues>({
    resolver: zodResolver(paymentSchema),
    defaultValues: {
      paymentMethod: "credit-card",
      cardNumber: "4242424242424242",
      expiry: "12/30",
      cvv: "123",
      checkNumber: "",
      cashReference: "",
    },
  });

  const paymentMethod = useWatch({
    control: form.control,
    name: "paymentMethod",
  });

  const invoice = invoiceQuery.data;

  const onDownloadPdf = async () => {
    if (!activeBookingId || !invoice) {
      return;
    }

    try {
      const blob = await downloadInvoicePdf(activeBookingId);
      const url = URL.createObjectURL(blob);
      const anchor = document.createElement("a");
      anchor.href = url;
      anchor.download = `${invoice.invoiceNumber}.pdf`;
      anchor.click();
      URL.revokeObjectURL(url);
    } catch (error) {
      addToast({
        title: "Unable to download invoice",
        description: getApiErrorMessage(error, "Invoice download failed."),
        tone: "warning",
      });
    }
  };

  const onSubmit = form.handleSubmit(async (values) => {
    if (!invoice) {
      return;
    }

    await paymentMutation.mutateAsync({
      bookingId: activeBookingId,
      paymentMethod: values.paymentMethod,
      amount: invoice.total,
    });
  });

  if (bookingsQuery.isLoading) {
    return <div className="py-16 text-sm text-muted-foreground">Loading payments...</div>;
  }

  if (bookingsQuery.isError) {
    return (
      <div className="rounded-[1.5rem] border border-accent-red/20 bg-accent-red/10 px-6 py-5 text-sm text-accent-red">
        {getApiErrorMessage(bookingsQuery.error, "Unable to load bookings for billing.")}
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <SectionHeading
        eyebrow="Phase 10"
        title="Payments & Invoices"
        description="Review invoice line items, collect payment, and download PDF invoices from the live billing endpoints."
      />

      <Card>
        <CardHeader>
          <CardTitle>Select booking</CardTitle>
          <CardDescription>
            Choose a booking to load its invoice and payment workflow.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Select
            value={activeBookingId}
            onChange={(event) => setSelectedBookingId(event.target.value)}
          >
            <option value="">Select booking</option>
            {availableBookings.map((booking) => (
              <option key={booking.recordId} value={booking.recordId}>
                {booking.id} - {booking.guestName} - Room {booking.roomNumber}
              </option>
            ))}
          </Select>
        </CardContent>
      </Card>

      {!activeBookingId ? (
        <div className="rounded-[1.5rem] border border-surface-border bg-white/[0.03] px-6 py-5 text-sm text-muted-foreground">
          No payable booking is available yet.
        </div>
      ) : null}

      {invoiceQuery.isError ? (
        <div className="rounded-[1.5rem] border border-accent-red/20 bg-accent-red/10 px-6 py-5 text-sm text-accent-red">
          {getApiErrorMessage(invoiceQuery.error, "Unable to load invoice details.")}
        </div>
      ) : null}

      {invoice ? (
        <div className="grid gap-6 xl:grid-cols-[minmax(0,1.55fr)_minmax(0,0.75fr)]">
          <Card>
            <CardHeader className="flex flex-row items-start justify-between gap-4">
              <div>
                <p className="text-xs uppercase tracking-[0.22em] text-muted-foreground">
                  Invoice
                </p>
                <CardTitle className="mt-3 text-4xl">{invoice.invoiceNumber}</CardTitle>
                <CardDescription className="mt-3">
                  Issued {formatCompactDate(invoice.issuedDate)}
                </CardDescription>
              </div>
              <div className="text-right text-sm whitespace-pre-line text-muted-foreground">
                <p className="font-semibold text-[#9ea7ff]">{invoice.hotelName}</p>
                {invoice.hotelAddress}
              </div>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="grid gap-4 rounded-[1.4rem] border border-surface-border bg-white/[0.03] p-5 md:grid-cols-2">
                <div className="space-y-3">
                  <div>
                    <p className="text-xs uppercase tracking-[0.2em] text-muted-foreground">
                      Guest
                    </p>
                    <p className="mt-2 text-lg font-semibold text-white">
                      {invoice.guestName}
                    </p>
                  </div>
                  <div>
                    <p className="text-xs uppercase tracking-[0.2em] text-muted-foreground">
                      Check-in
                    </p>
                    <p className="mt-2 text-sm text-slate-200">
                      {formatCompactDate(invoice.checkIn)}
                    </p>
                  </div>
                </div>
                <div className="space-y-3">
                  <div>
                    <p className="text-xs uppercase tracking-[0.2em] text-muted-foreground">
                      Booking ID
                    </p>
                    <p className="mt-2 text-lg font-semibold text-white">
                      {
                        availableBookings.find((booking) => booking.recordId === activeBookingId)
                          ?.id
                      }
                    </p>
                  </div>
                  <div>
                    <p className="text-xs uppercase tracking-[0.2em] text-muted-foreground">
                      Check-out
                    </p>
                    <p className="mt-2 text-sm text-slate-200">
                      {formatCompactDate(invoice.checkOut)}
                    </p>
                  </div>
                </div>
              </div>

              <div className="space-y-3">
                {invoice.items.map((item) => (
                  <div
                    key={item.id}
                    className="grid gap-3 border-b border-surface-border pb-3 md:grid-cols-[minmax(0,1fr)_auto_auto]"
                  >
                    <p className="text-sm text-slate-200">{item.description}</p>
                    <p className="text-sm text-muted-foreground">{item.rateLabel}</p>
                    <p className="text-sm font-semibold text-white">
                      {formatPreciseCurrency(item.total)}
                    </p>
                  </div>
                ))}
              </div>

              <div className="space-y-2 text-right">
                <p className="text-sm text-muted-foreground">
                  Subtotal: {formatPreciseCurrency(invoice.subtotal)}
                </p>
                <p className="text-sm text-muted-foreground">
                  Tax: {formatPreciseCurrency(invoice.tax)}
                </p>
                <div className="rounded-[1.25rem] border border-accent-indigo/25 bg-accent-indigo/10 px-4 py-4 text-left">
                  <p className="text-sm font-semibold text-white">Total Due</p>
                  <p className="mt-2 text-3xl font-bold text-accent-green">
                    {formatPreciseCurrency(invoice.total)}
                  </p>
                </div>
              </div>

              <div className="flex flex-wrap gap-3 print:hidden">
                <Button variant="secondary" onClick={() => void onDownloadPdf()}>
                  <Download className="size-4" />
                  Download PDF
                </Button>
                <Button variant="secondary" onClick={() => window.print()}>
                  <Printer className="size-4" />
                  Print Invoice
                </Button>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Payment method</CardTitle>
              <CardDescription>
                Capture payment details and process settlement from the invoice panel.
              </CardDescription>
            </CardHeader>
            <CardContent>
              <form onSubmit={onSubmit} className="space-y-4">
                <div className="space-y-3">
                  {(["credit-card", "cash", "check"] as const).map((method) => (
                    <button
                      key={method}
                      type="button"
                      onClick={() => form.setValue("paymentMethod", method)}
                      className={`flex w-full items-center justify-between rounded-[1.25rem] border px-4 py-4 text-left transition ${
                        paymentMethod === method
                          ? "border-accent-indigo/35 bg-accent-indigo/16 text-white"
                          : "border-surface-border bg-white/[0.03] text-muted-foreground"
                      }`}
                    >
                      <span className="text-sm font-semibold">
                        {method.replaceAll("-", " ")}
                      </span>
                      <span className="size-4 rounded-full border border-current" />
                    </button>
                  ))}
                </div>

                {paymentMethod === "credit-card" ? (
                  <>
                    <div className="space-y-2">
                      <label className="text-xs uppercase tracking-[0.2em] text-muted-foreground">
                        Card number
                      </label>
                      <Input {...form.register("cardNumber")} />
                      {form.formState.errors.cardNumber ? (
                        <p className="text-sm text-accent-red">
                          {form.formState.errors.cardNumber.message}
                        </p>
                      ) : null}
                    </div>
                    <div className="grid gap-4 md:grid-cols-2">
                      <div className="space-y-2">
                        <label className="text-xs uppercase tracking-[0.2em] text-muted-foreground">
                          Expiry
                        </label>
                        <Input {...form.register("expiry")} />
                        {form.formState.errors.expiry ? (
                          <p className="text-sm text-accent-red">
                            {form.formState.errors.expiry.message}
                          </p>
                        ) : null}
                      </div>
                      <div className="space-y-2">
                        <label className="text-xs uppercase tracking-[0.2em] text-muted-foreground">
                          CVV
                        </label>
                        <Input {...form.register("cvv")} />
                        {form.formState.errors.cvv ? (
                          <p className="text-sm text-accent-red">
                            {form.formState.errors.cvv.message}
                          </p>
                        ) : null}
                      </div>
                    </div>
                  </>
                ) : null}

                {paymentMethod === "cash" ? (
                  <div className="space-y-2">
                    <label className="text-xs uppercase tracking-[0.2em] text-muted-foreground">
                      Cash reference
                    </label>
                    <Input
                      placeholder="Drawer batch or receipt ref"
                      {...form.register("cashReference")}
                    />
                    {form.formState.errors.cashReference ? (
                      <p className="text-sm text-accent-red">
                        {form.formState.errors.cashReference.message}
                      </p>
                    ) : null}
                  </div>
                ) : null}

                {paymentMethod === "check" ? (
                  <div className="space-y-2">
                    <label className="text-xs uppercase tracking-[0.2em] text-muted-foreground">
                      Check number
                    </label>
                    <Input placeholder="CHK-100291" {...form.register("checkNumber")} />
                    {form.formState.errors.checkNumber ? (
                      <p className="text-sm text-accent-red">
                        {form.formState.errors.checkNumber.message}
                      </p>
                    ) : null}
                  </div>
                ) : null}

                <div className="flex items-center justify-between py-3">
                  <p className="text-muted-foreground">Total</p>
                  <p className="text-3xl font-bold text-accent-green">
                    {formatPreciseCurrency(invoice.total)}
                  </p>
                </div>

                <Button
                  type="submit"
                  className="w-full"
                  size="lg"
                  disabled={!canManagePayments || paymentMutation.isPending}
                >
                  {paymentMutation.isPending ? "Processing..." : "Process payment"}
                </Button>
              </form>
            </CardContent>
          </Card>
        </div>
      ) : invoiceQuery.isLoading ? (
        <div className="py-16 text-sm text-muted-foreground">Loading invoice...</div>
      ) : null}
    </div>
  );
}
