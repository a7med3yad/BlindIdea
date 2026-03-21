import { z } from 'zod';

export const createTeamSchema = z.object({
  name: z.string()
    .min(3, 'Team name must be at least 3 characters')
    .max(50, 'Team name must be less than 50 characters')
    .trim(),
});

export const joinTeamSchema = z.object({
  inviteCode: z.string()
    .length(8, 'Invite code must be exactly 8 characters')
    .regex(/^[A-Z0-9]+$/, 'Uppercase letters and numbers only')
    .transform((val) => val.toUpperCase()),
});

export type CreateTeamFormData = z.infer<typeof createTeamSchema>;
export type JoinTeamFormData = z.infer<typeof joinTeamSchema>;
