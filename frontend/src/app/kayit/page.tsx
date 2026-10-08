import type { Metadata } from "next";
import Link from "next/link";
import { redirect } from "next/navigation";
import { AuthShell, Field, Submit } from "@/components/auth-form";
import { signUp } from "@/lib/auth";

export const metadata: Metadata = {
  title: "Kayıt ol",
  robots: { index: false, follow: false },
};

type Params = { searchParams: Promise<{ hata?: string }> };

export default async function RegisterPage({ searchParams }: Params) {
  const { hata } = await searchParams;

  async function kayitOl(formData: FormData): Promise<void> {
    "use server";

    const email = String(formData.get("email") ?? "").trim();
    const password = String(formData.get("password") ?? "");
    const fullName = String(formData.get("fullName") ?? "").trim();

    const sonuc = await signUp(email, password, fullName || null);

    if (!sonuc.ok) redirect(`/kayit?hata=${encodeURIComponent(sonuc.message)}`);

    // Yeni kullanıcının tercihi yok; sihirbaz akış sayfasında karşılıyor.
    redirect("/akis");
  }

  return (
    <AuthShell
      title="Kayıt ol"
      hata={hata}
      footer={
        <>
          Hesabın var mı?{" "}
          <Link href="/giris" className="text-amber underline-offset-2 hover:underline">
            Giriş yap
          </Link>
        </>
      }
    >
      <form action={kayitOl} className="mt-6 space-y-3">
        <Field label="Ad soyad" name="fullName" type="text" autoComplete="name" required={false} />
        <Field label="E-posta" name="email" type="email" autoComplete="username" />
        <Field
          label="Parola"
          name="password"
          type="password"
          autoComplete="new-password"
          hint="En az 8 karakter."
        />

        <Submit>Hesap oluştur</Submit>
      </form>

      <p className="mt-4 text-xs leading-relaxed text-fog">
        Hesap açarak e-posta adresinin ve takip ettiğin takım, branş ve yorumcuların
        saklanmasını kabul ediyorsun. Bu bilgiler yalnızca sana özel bülten hazırlamak
        için kullanılıyor.
      </p>
    </AuthShell>
  );
}
