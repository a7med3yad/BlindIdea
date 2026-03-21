import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Send } from 'lucide-react';
import Input from '../../ui/Input';
import Button from '../../ui/Button';
import { submitIdeaSchema, type SubmitIdeaFormData } from '../../../schemas/idea.schema';
import { useSubmitIdea } from '../../../hooks/useIdeas';

export default function SubmitIdeaForm() {
  const {
    register,
    handleSubmit,
    watch,
    reset,
    formState: { errors, isValid },
  } = useForm<SubmitIdeaFormData>({
    resolver: zodResolver(submitIdeaSchema),
    mode: 'onChange',
  });

  const { mutate, isPending } = useSubmitIdea();
  const title = watch('title', '');
  const content = watch('content', '');

  const onSubmit = (data: SubmitIdeaFormData) => {
    mutate(data, { onSuccess: () => reset() });
  };

  return (
    <div className="bg-[#0D0D0D] border border-[#2A2A2A] rounded-2xl p-6">
      <div className="flex items-center gap-2 mb-6">
        <Send className="w-5 h-5 text-[#E8003D]" />
        <h2 className="text-lg font-bold text-white">Submit an Idea</h2>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <div>
          <Input
            label="Title"
            placeholder="What's your idea?"
            error={errors.title?.message}
            disabled={isPending}
            {...register('title')}
          />
          <p className={`text-xs text-right mt-1 ${title.length > 100 ? 'text-[#EF4444]' : 'text-[#555555]'}`}>
            {title.length}/100
          </p>
        </div>

        <div>
          <label className="block text-sm font-medium text-[#AAAAAA] mb-2">
            Content
          </label>
          <textarea
            placeholder="Describe your idea in detail (min. 20 characters)"
            disabled={isPending}
            {...register('content')}
            rows={5}
            className={`
              w-full min-h-[140px] bg-[#1A1A1A] border text-white rounded-lg
              px-4 py-3 text-base placeholder:text-[#555555]
              transition-all duration-200 resize-none
              ${
                errors.content
                  ? 'border-[#EF4444] focus:border-[#EF4444] focus:ring-1 focus:ring-[#EF4444]/50'
                  : 'border-[#2A2A2A] hover:border-[#3A3A3A] focus:border-[#E8003D] focus:ring-1 focus:ring-[#E8003D]/50'
              }
              focus:outline-none disabled:opacity-50
            `}
          />
          <div className="flex justify-between mt-1">
            {errors.content && (
              <p className="text-xs text-[#EF4444]">{errors.content.message}</p>
            )}
            <p className={`text-xs ml-auto ${content.length > 1000 ? 'text-[#EF4444]' : 'text-[#555555]'}`}>
              {content.length}/1000
            </p>
          </div>
        </div>

        <Button
          type="submit"
          fullWidth
          isLoading={isPending}
          disabled={!isValid || isPending}
          className="mt-2"
        >
          <Send className="w-4 h-4" />
          Submit Anonymously
        </Button>
      </form>
    </div>
  );
}
