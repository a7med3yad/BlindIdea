import { z } from 'zod';

export const submitIdeaSchema = z.object({
  title: z.string()
    .min(5, 'Title must be at least 5 characters')
    .max(100, 'Title must be less than 100 characters')
    .trim(),
  content: z.string()
    .min(20, 'Content must be at least 20 characters')
    .max(1000, 'Content must be less than 1000 characters')
    .trim(),
});

export const rateIdeaSchema = z.object({
  score: z.number()
    .min(1, 'Rating must be at least 1')
    .max(5, 'Rating must be at most 5')
    .int('Rating must be a whole number'),
});

export type SubmitIdeaFormData = z.infer<typeof submitIdeaSchema>;
export type RateIdeaFormData = z.infer<typeof rateIdeaSchema>;
