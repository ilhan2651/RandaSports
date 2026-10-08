import type { Metadata } from "next";
import Link from "next/link";
import { redirect } from "next/navigation";
import { AuthShell, Field, Submit } from "@/components/auth-form";
import { signIn } from "@/lib/auth";

export const metadata: Metadata = {
  title: "Giriş",
  robots: { index: false, follow: false },
};

type Params = { searchParams: Promise<{ devam?: string; hata?: string }> };

export default async function LoginPage({ searchParams }: Params) {
  const { devam, hata } = await searchParams;

  async function girisYap(formData: FormData): Promise<void> {
    "use server";

    const email = String(formData.get("email") ?? "").trim();
    const password = String(formData.get("password") ?? "");
    const hedef = String(formData.get("devam") ?? "");

    const sonuc = await signIn(email, password);

    if (!sonuc.ok) {
      const params = new URLSearchParams({ hata: sonuc.message });
      if (hedef) params.set("devam", hedef);
      redirect(`/giris?${params}`);
    }

    // Açık yönlendirmeyi engelliyoruz: yalnızca kendi yollarımıza dönüyoruz.
    if (hedef.startsWith("/") && !hedef.startsWith("//")) redirect(hedef);

    // Herkes akışa gidiyor: giriş yapmanın karşılığı kişiselleşmiş akış. Yönetim
    // paneline bağlantı hesap sayfasında duruyor, girişin varış noktası değil.
    redirect("/akis");
  }

  return (
    <AuthShell
      title="Giriş yap"
      hata={hata}
      footer={
        <>
          Hesabın yok mu?{" "}
          <Link href="/kayit" className="text-amber underline-offset-2 hover:underline">
            Kayıt ol
          </Link>
        </>
      }
    >
      <form action={girisYap} className="mt-6 space-y-3">
        <input type="hidden" name="devam" value={devam ?? ""} />

        <Field label="E-posta" name="email" type="email" autoComplete="username" autoFocus />
        <Field label="Parola" name="password" type="password" autoComplete="current-password" />

        <Submit>Giriş yap</Submit>
      </form>
    </AuthShell>
  );
}
